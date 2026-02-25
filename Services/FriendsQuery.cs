using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query implementation for friend list and user search.
    /// </summary>
    public class FriendsQuery : IFriendsQuery
    {
        private const int MaxTake = 25;

        private readonly ApplicationDbContext _context;

        public FriendsQuery(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<FriendListItemDto>> GetFriendsAsync(string userId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var friends = await _context.FriendRelationships
                .Where(fr => fr.UserId == userId)
                .Join(
                    _context.Users,
                    fr => fr.FriendUserId,
                    u => u.Id,
                    (fr, u) => new { FriendUser = u })
                .GroupJoin(
                    _context.UserGirls.Join(
                        _context.Girls,
                        ug => ug.GirlId,
                        g => g.GirlId,
                        (ug, g) => new { ug.UserId, ug.GirlId, ug.Bond, g.Name, g.ImageUrl }),
                    f => new { UserId = f.FriendUser.Id, GirlId = f.FriendUser.PartnerGirlId ?? 0 },
                    pg => new { pg.UserId, pg.GirlId },
                    (f, partnerGroup) => new { f.FriendUser, PartnerGroup = partnerGroup })
                .SelectMany(
                    x => x.PartnerGroup.DefaultIfEmpty(),
                    (x, partner) => new FriendListItemDto
                    {
                        UserId = x.FriendUser.Id,
                        DisplayName = x.FriendUser.DisplayName,
                        PartnerName = partner != null ? partner.Name : null,
                        PartnerImagePath = partner != null ? partner.ImageUrl : null,
                        PartnerBond = partner != null ? partner.Bond : null
                    })
                .OrderBy(f => f.DisplayName)
                .ToListAsync(ct);

            return friends;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserSearchResultDto>> SearchUsersByDisplayNameAsync(
            string requesterUserId, string searchText, int take, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(requesterUserId);

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return [];
            }

            var normalized = searchText.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(normalized))
            {
                return [];
            }

            var clampedTake = Math.Clamp(take, 1, MaxTake);

            // Get the requester's existing friend IDs for IsAlreadyFriend marking.
            var existingFriendIds = await _context.FriendRelationships
                .Where(fr => fr.UserId == requesterUserId)
                .Select(fr => fr.FriendUserId)
                .ToListAsync(ct);

            var friendIdSet = new HashSet<string>(existingFriendIds);

            var matchedUsers = await _context.Users
                .Where(u => u.Id != requesterUserId
                    && u.DisplayNameNormalized.StartsWith(normalized))
                .OrderBy(u => u.DisplayNameNormalized)
                .Take(clampedTake)
                .Select(u => new
                {
                    u.Id,
                    u.DisplayName,
                    u.PartnerGirlId
                })
                .ToListAsync(ct);

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

            var results = matchedUsers.Select(u =>
            {
                var isAlreadyFriend = friendIdSet.Contains(u.Id);
                string? partnerImagePath = u.PartnerGirlId.HasValue
                    && partnerImages.TryGetValue(u.PartnerGirlId.Value, out var img)
                    ? img
                    : null;

                return new UserSearchResultDto
                {
                    UserId = u.Id,
                    DisplayName = u.DisplayName,
                    PartnerImagePath = partnerImagePath,
                    IsAlreadyFriend = isAlreadyFriend,
                    CanAdd = !isAlreadyFriend
                };
            }).ToList();

            return results;
        }
    }
}
