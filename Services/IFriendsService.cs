namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Write service for managing friend relationships (add/remove).
    /// </summary>
    public interface IFriendsService
    {
        /// <summary>
        /// Attempts to add a bidirectional friend relationship between two users.
        /// </summary>
        /// <param name="userId">The initiating user's ID.</param>
        /// <param name="friendUserId">The target user's ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An <see cref="AddFriendResult"/> indicating success or the reason for failure.</returns>
        Task<AddFriendResult> TryAddFriendAsync(string userId, string friendUserId, CancellationToken ct);
    }
}
