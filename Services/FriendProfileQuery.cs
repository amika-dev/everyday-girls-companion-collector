using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query implementation for viewing a friend's profile.
    /// </summary>
    public class FriendProfileQuery : IFriendProfileQuery
    {
        private readonly ApplicationDbContext _context;
        private readonly IClock _clock;

        public FriendProfileQuery(ApplicationDbContext context, IClock clock)
        {
            _context = context;
            _clock = clock;
        }

        /// <inheritdoc />
        public async Task<FriendProfileViewModel?> GetFriendProfileAsync(
            string viewerUserId, string friendUserId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(viewerUserId);
            ArgumentException.ThrowIfNullOrWhiteSpace(friendUserId);

            var friendUser = await _context.Users
                .Include(u => u.Partner)
                .FirstOrDefaultAsync(u => u.Id == friendUserId, ct);

            if (friendUser is null)
            {
                return null;
            }

            var userGirls = await _context.UserGirls
                .Where(ug => ug.UserId == friendUserId)
                .ToListAsync(ct);

            var totalBond = userGirls.Sum(ug => ug.Bond);
            var companionsCount = userGirls.Count;

            var currentServerDate = DailyCadence.GetServerDateFromUtc(_clock.UtcNow);
            var partnerData = friendUser.PartnerGirlId.HasValue
                ? userGirls.FirstOrDefault(ug => ug.GirlId == friendUser.PartnerGirlId.Value)
                : null;

            return new FriendProfileViewModel
            {
                FriendUserId = friendUser.Id,
                DisplayName = friendUser.DisplayName,
                PartnerName = friendUser.Partner?.Name,
                PartnerImagePath = friendUser.Partner?.ImageUrl,
                PartnerBond = partnerData?.Bond,
                PartnerFirstMetUtc = partnerData?.DateMetUtc,
                PartnerDaysTogether = partnerData is not null
                    ? DailyCadence.GetDaysSinceAdoption(currentServerDate, partnerData.DateMetUtc)
                    : null,
                PartnerPersonalityTag = partnerData?.PersonalityTag,
                CompanionsCount = companionsCount,
                TotalBond = totalBond
            };
        }
    }
}
