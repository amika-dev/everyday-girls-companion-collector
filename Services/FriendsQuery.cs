using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query implementation for friend list and user search with paging.
    /// </summary>
    public class FriendsQuery : IFriendsQuery
    {
        private readonly ApplicationDbContext _context;

        public FriendsQuery(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<PagedResult<FriendListItemDto>> GetFriendsAsync(
            string userId, int page, int pageSize, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var (clampedPage, clampedPageSize) = PagedResult<FriendListItemDto>.Clamp(
                page, pageSize, GameConstants.FriendsPageSize);

            // Count total friends.
            var totalCount = await _context.FriendRelationships
                .CountAsync(fr => fr.UserId == userId, ct);

            if (totalCount == 0)
            {
                return PagedResult<FriendListItemDto>.Empty(clampedPage, clampedPageSize);
            }

            // Fetch the friend user IDs for this page, ordered by DisplayName.
            var friendUsers = await _context.FriendRelationships
                .Where(fr => fr.UserId == userId)
                .Join(
                    _context.Users,
                    fr => fr.FriendUserId,
                    u => u.Id,
                    (fr, u) => u)
                .OrderBy(u => u.DisplayNameNormalized)
                .ThenBy(u => u.DisplayName)
                .Skip((clampedPage - 1) * clampedPageSize)
                .Take(clampedPageSize)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayName,
                    u.PartnerGirlId
                })
                .ToListAsync(ct);

            var friendUserIds = friendUsers.Select(u => u.Id).ToList();

            // Batch-fetch companion stats (count + total bond) for page users.
            var companionStats = await _context.UserGirls
                .Where(ug => friendUserIds.Contains(ug.UserId))
                .GroupBy(ug => ug.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    TotalBond = g.Sum(ug => ug.Bond)
                })
                .ToDictionaryAsync(x => x.UserId, ct);

            // Batch-fetch partner details for users that have a partner.
            var partnerLookups = friendUsers
                .Where(u => u.PartnerGirlId.HasValue)
                .Select(u => new { u.Id, PartnerGirlId = u.PartnerGirlId!.Value })
                .ToList();

            var partnerDetails = new Dictionary<string, (string Name, string ImageUrl, int Bond)>();

            if (partnerLookups.Count > 0)
            {
                foreach (var lookup in partnerLookups)
                {
                    var partner = await _context.UserGirls
                        .Where(ug => ug.UserId == lookup.Id && ug.GirlId == lookup.PartnerGirlId)
                        .Join(
                            _context.Girls,
                            ug => ug.GirlId,
                            g => g.GirlId,
                            (ug, g) => new { g.Name, g.ImageUrl, ug.Bond })
                        .FirstOrDefaultAsync(ct);

                    if (partner is not null)
                    {
                        partnerDetails[lookup.Id] = (partner.Name, partner.ImageUrl, partner.Bond);
                    }
                }
            }

            var items = friendUsers.Select(u =>
            {
                companionStats.TryGetValue(u.Id, out var stats);
                partnerDetails.TryGetValue(u.Id, out var partner);

                return new FriendListItemDto
                {
                    UserId = u.Id,
                    DisplayName = u.DisplayName,
                    PartnerName = partner.Name,
                    PartnerImagePath = partner.ImageUrl,
                    PartnerBond = partner.Name is not null ? partner.Bond : null,
                    CompanionsCount = stats?.Count ?? 0,
                    TotalBond = stats?.TotalBond ?? 0
                };
            }).ToList();

            return new PagedResult<FriendListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedPageSize
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<UserSearchResultDto>> SearchUsersByDisplayNameAsync(
            string requesterUserId, string searchText, int page, int pageSize, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requesterUserId);

            var (clampedPage, clampedPageSize) = PagedResult<UserSearchResultDto>.Clamp(
                page, pageSize, GameConstants.FriendsPageSize);

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return PagedResult<UserSearchResultDto>.Empty(clampedPage, clampedPageSize);
            }

            var normalized = searchText.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(normalized))
            {
                return PagedResult<UserSearchResultDto>.Empty(clampedPage, clampedPageSize);
            }

            // Base query: matching users excluding self.
            var baseQuery = _context.Users
                .Where(u => u.Id != requesterUserId
                    && u.DisplayNameNormalized.StartsWith(normalized));

            var totalCount = await baseQuery.CountAsync(ct);

            if (totalCount == 0)
            {
                return PagedResult<UserSearchResultDto>.Empty(clampedPage, clampedPageSize);
            }

            // Get the requester's existing friend IDs for IsAlreadyFriend marking.
            var existingFriendIds = await _context.FriendRelationships
                .Where(fr => fr.UserId == requesterUserId)
                .Select(fr => fr.FriendUserId)
                .ToListAsync(ct);

            var friendIdSet = new HashSet<string>(existingFriendIds);

            var matchedUsers = await baseQuery
                .OrderBy(u => u.DisplayNameNormalized)
                .ThenBy(u => u.DisplayName)
                .Skip((clampedPage - 1) * clampedPageSize)
                .Take(clampedPageSize)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayName,
                    u.PartnerGirlId
                })
                .ToListAsync(ct);

            var matchedUserIds = matchedUsers.Select(u => u.Id).ToList();

            // Batch-fetch companion stats for matched users.
            var companionStats = await _context.UserGirls
                .Where(ug => matchedUserIds.Contains(ug.UserId))
                .GroupBy(ug => ug.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    TotalBond = g.Sum(ug => ug.Bond)
                })
                .ToDictionaryAsync(x => x.UserId, ct);

            // Batch-fetch partner images for users that have a partner.
            var partnerGirlIds = matchedUsers
                .Where(u => u.PartnerGirlId.HasValue)
                .Select(u => u.PartnerGirlId!.Value)
                .Distinct()
                .ToList();

            var partnerImages = partnerGirlIds.Count > 0
                ? await _context.Girls
                    .Where(g => partnerGirlIds.Contains(g.GirlId))
                    .ToDictionaryAsync(g => g.GirlId, g => g.ImageUrl, ct)
                : new Dictionary<int, string>();

            var items = matchedUsers.Select(u =>
            {
                var isAlreadyFriend = friendIdSet.Contains(u.Id);
                string? partnerImagePath = u.PartnerGirlId.HasValue
                    && partnerImages.TryGetValue(u.PartnerGirlId.Value, out var img)
                    ? img
                    : null;
                companionStats.TryGetValue(u.Id, out var stats);

                return new UserSearchResultDto
                {
                    UserId = u.Id,
                    DisplayName = u.DisplayName,
                    PartnerImagePath = partnerImagePath,
                    IsAlreadyFriend = isAlreadyFriend,
                    CanAdd = !isAlreadyFriend,
                    CompanionsCount = stats?.Count ?? 0,
                    TotalBond = stats?.TotalBond ?? 0
                };
            }).ToList();

            return new PagedResult<UserSearchResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedPageSize
            };
        }
    }
}
