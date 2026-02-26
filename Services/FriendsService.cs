using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Write service for creating bidirectional friend relationships.
    /// </summary>
    public class FriendsService : IFriendsService
    {
        private readonly ApplicationDbContext _context;

        public FriendsService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<AddFriendResult> TryAddFriendAsync(
            string userId, string friendUserId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(friendUserId);

            // Cannot add self.
            if (userId == friendUserId)
            {
                return AddFriendResult.CreateFailure("CannotAddSelf", "You can't add yourself as a friend.");
            }

            // Target user must exist.
            var friendExists = await _context.Users.AnyAsync(u => u.Id == friendUserId, ct);
            if (!friendExists)
            {
                return AddFriendResult.CreateFailure("UserNotFound", "That user doesn't exist.");
            }

            // Must not already be friends.
            var alreadyFriends = await _context.FriendRelationships
                .AnyAsync(fr => fr.UserId == userId && fr.FriendUserId == friendUserId, ct);

            if (alreadyFriends)
            {
                return AddFriendResult.CreateFailure("AlreadyFriends", "You're already friends with this user.");
            }

            var now = DateTime.UtcNow;

            var rowA = new FriendRelationship
            {
                UserId = userId,
                FriendUserId = friendUserId,
                DateAddedUtc = now
            };
            var rowB = new FriendRelationship
            {
                UserId = friendUserId,
                FriendUserId = userId,
                DateAddedUtc = now
            };

            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async (cancellation) =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellation);
                    _context.FriendRelationships.Add(rowA);
                    _context.FriendRelationships.Add(rowB);
                    await _context.SaveChangesAsync(cancellation);
                    await transaction.CommitAsync(cancellation);
                }, ct);
            }
            catch (DbUpdateException)
            {
                // Race condition: another request inserted rows concurrently.
                return AddFriendResult.CreateFailure("AlreadyFriends", "You're already friends with this user.");
            }

            return AddFriendResult.CreateSuccess();
        }

        /// <inheritdoc />
        public async Task<RemoveFriendResult> TryRemoveFriendAsync(
            string userId, string friendUserId, CancellationToken ct)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            ArgumentException.ThrowIfNullOrWhiteSpace(friendUserId);

            if (userId == friendUserId)
            {
                return RemoveFriendResult.CreateFailure("CannotRemoveSelf", "You can't remove yourself as a friend.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async (cancellation) =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellation);

                var rowsAtoB = await _context.FriendRelationships
                    .Where(fr => fr.UserId == userId && fr.FriendUserId == friendUserId)
                    .ToListAsync(cancellation);

                var rowsBtoA = await _context.FriendRelationships
                    .Where(fr => fr.UserId == friendUserId && fr.FriendUserId == userId)
                    .ToListAsync(cancellation);

                var totalFound = rowsAtoB.Count + rowsBtoA.Count;

                if (totalFound == 0)
                {
                    await transaction.RollbackAsync(cancellation);
                    return RemoveFriendResult.CreateFailure("NotFriends", "You're not friends with this user.");
                }

                _context.FriendRelationships.RemoveRange(rowsAtoB);
                _context.FriendRelationships.RemoveRange(rowsBtoA);
                await _context.SaveChangesAsync(cancellation);
                await transaction.CommitAsync(cancellation);

                return RemoveFriendResult.CreateSuccess();
            }, ct);
        }
    }
}
