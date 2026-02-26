using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query for viewing a friend's companion collection.
    /// </summary>
    public interface IFriendCollectionQuery
    {
        /// <summary>
        /// Returns a paged list of companions in a friend's collection, ordered by bond descending then date met.
        /// </summary>
        /// <param name="viewerUserId">The viewing user's ID (not used for authorization, reserved for future use).</param>
        /// <param name="friendUserId">The friend whose collection to retrieve.</param>
        /// <param name="page">1-based page number. Values &lt; 1 are clamped to 1.</param>
        /// <param name="pageSize">Items per page. Values &lt;= 0 use the default friends page size.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PagedResult<FriendGirlListItemDto>> GetFriendCollectionAsync(
            string viewerUserId, string friendUserId, int page, int pageSize, CancellationToken ct);

        /// <summary>
        /// Returns immutable detail data for a single companion in a friend's collection.
        /// Returns null if the friend does not own the specified companion.
        /// </summary>
        /// <param name="viewerUserId">The viewing user's ID (not used for authorization, reserved for future use).</param>
        /// <param name="friendUserId">The friend who owns the companion.</param>
        /// <param name="girlId">The girl to retrieve details for.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<FriendGirlDetailsDto?> GetFriendGirlDetailsAsync(
            string viewerUserId, string friendUserId, int girlId, CancellationToken ct);
    }
}
