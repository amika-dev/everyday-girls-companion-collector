using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only queries for the friends system (friend list and user search).
    /// </summary>
    public interface IFriendsQuery
    {
        /// <summary>
        /// Returns a paged friend list for the given user, ordered by display name ascending.
        /// </summary>
        /// <param name="userId">The user whose friends to retrieve.</param>
        /// <param name="page">1-based page number. Values < 1 are clamped to 1.</param>
        /// <param name="pageSize">Items per page. Values <= 0 use the default friends page size.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PagedResult<FriendListItemDto>> GetFriendsAsync(string userId, int page, int pageSize, CancellationToken ct);

        /// <summary>
        /// Searches users by display name using starts-with matching, with paging.
        /// Excludes the requester. Marks each result with friendship status.
        /// </summary>
        /// <param name="requesterUserId">The searching user's ID (excluded from results).</param>
        /// <param name="searchText">Display name prefix to search for (trimmed internally).</param>
        /// <param name="page">1-based page number. Values < 1 are clamped to 1.</param>
        /// <param name="pageSize">Items per page. Values <= 0 use the default friends page size.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PagedResult<UserSearchResultDto>> SearchUsersByDisplayNameAsync(
            string requesterUserId, string searchText, int page, int pageSize, CancellationToken ct);
    }
}
