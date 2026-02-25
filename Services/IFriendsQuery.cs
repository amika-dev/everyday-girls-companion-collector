using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only queries for the friends system (friend list and user search).
    /// </summary>
    public interface IFriendsQuery
    {
        /// <summary>
        /// Returns the friend list for the given user, ordered by display name ascending.
        /// </summary>
        /// <param name="userId">The user whose friends to retrieve.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<FriendListItemDto>> GetFriendsAsync(string userId, CancellationToken ct);

        /// <summary>
        /// Searches users by display name using starts-with matching.
        /// Excludes the requester. Marks each result with friendship status.
        /// </summary>
        /// <param name="requesterUserId">The searching user's ID (excluded from results).</param>
        /// <param name="searchText">Display name prefix to search for (trimmed internally).</param>
        /// <param name="take">Maximum number of results to return (clamped to 25).</param>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<UserSearchResultDto>> SearchUsersByDisplayNameAsync(
            string requesterUserId, string searchText, int take, CancellationToken ct);
    }
}
