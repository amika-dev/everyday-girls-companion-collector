using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query implementation for viewing a friend's companion collection.
    /// </summary>
    public class FriendCollectionQuery : IFriendCollectionQuery
    {
        private readonly ApplicationDbContext _context;
        private readonly IClock _clock;

        public FriendCollectionQuery(ApplicationDbContext context, IClock clock)
        {
            _context = context;
            _clock = clock;
        }

        /// <inheritdoc />
        public async Task<PagedResult<FriendGirlListItemDto>> GetFriendCollectionAsync(
            string viewerUserId, string friendUserId, int page, int pageSize, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(viewerUserId);
            ArgumentException.ThrowIfNullOrWhiteSpace(friendUserId);

            var (clampedPage, clampedPageSize) = PagedResult<FriendGirlListItemDto>.Clamp(
                page, pageSize, GameConstants.FriendsPageSize);

            var totalCount = await _context.UserGirls
                .CountAsync(ug => ug.UserId == friendUserId, ct);

            if (totalCount == 0)
            {
                return PagedResult<FriendGirlListItemDto>.Empty(clampedPage, clampedPageSize);
            }

            // Determine friend's partner for IsPartner marking.
            var partnerGirlId = await _context.Users
                .Where(u => u.Id == friendUserId)
                .Select(u => u.PartnerGirlId)
                .FirstOrDefaultAsync(ct);

            var currentServerDate = DailyCadence.GetServerDateFromUtc(_clock.UtcNow);

            var items = await _context.UserGirls
                .Where(ug => ug.UserId == friendUserId)
                .Join(
                    _context.Girls,
                    ug => ug.GirlId,
                    g => g.GirlId,
                    (ug, g) => new { UserGirl = ug, Girl = g })
                .OrderByDescending(x => x.UserGirl.Bond)
                .ThenBy(x => x.UserGirl.DateMetUtc)
                .Skip((clampedPage - 1) * clampedPageSize)
                .Take(clampedPageSize)
                .Select(x => new
                {
                    x.Girl.GirlId,
                    x.Girl.Name,
                    x.Girl.ImageUrl,
                    x.UserGirl.Bond,
                    x.UserGirl.PersonalityTag,
                    x.UserGirl.DateMetUtc
                })
                .ToListAsync(ct);

            var dtos = items.Select(x => new FriendGirlListItemDto
            {
                GirlId = x.GirlId,
                Name = x.Name,
                ImageUrl = x.ImageUrl,
                Bond = x.Bond,
                PersonalityTag = x.PersonalityTag,
                DaysTogether = DailyCadence.GetDaysSinceAdoption(currentServerDate, x.DateMetUtc),
                IsPartner = partnerGirlId.HasValue && x.GirlId == partnerGirlId.Value
            }).ToList();

            return new PagedResult<FriendGirlListItemDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedPageSize
            };
        }

        /// <inheritdoc />
        public async Task<FriendGirlDetailsDto?> GetFriendGirlDetailsAsync(
            string viewerUserId, string friendUserId, int girlId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(viewerUserId);
            ArgumentException.ThrowIfNullOrWhiteSpace(friendUserId);

            var data = await _context.UserGirls
                .Where(ug => ug.UserId == friendUserId && ug.GirlId == girlId)
                .Join(
                    _context.Girls,
                    ug => ug.GirlId,
                    g => g.GirlId,
                    (ug, g) => new { UserGirl = ug, Girl = g })
                .FirstOrDefaultAsync(ct);

            if (data is null)
            {
                return null;
            }

            var partnerGirlId = await _context.Users
                .Where(u => u.Id == friendUserId)
                .Select(u => u.PartnerGirlId)
                .FirstOrDefaultAsync(ct);

            var currentServerDate = DailyCadence.GetServerDateFromUtc(_clock.UtcNow);

            return new FriendGirlDetailsDto
            {
                GirlId = data.Girl.GirlId,
                Name = data.Girl.Name,
                ImageUrl = data.Girl.ImageUrl,
                Bond = data.UserGirl.Bond,
                PersonalityTag = data.UserGirl.PersonalityTag,
                DateMetUtc = data.UserGirl.DateMetUtc,
                DaysTogether = DailyCadence.GetDaysSinceAdoption(currentServerDate, data.UserGirl.DateMetUtc),
                IsPartner = partnerGirlId.HasValue && data.Girl.GirlId == partnerGirlId.Value,
                Charm = data.UserGirl.Charm,
                Focus = data.UserGirl.Focus,
                Vitality = data.UserGirl.Vitality
            };
        }
    }
}
