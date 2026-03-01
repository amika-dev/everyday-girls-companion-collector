using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only queries for the leaderboard system.
    /// </summary>
    public interface ILeaderboardQuery
    {
        /// <summary>
        /// Returns a paged leaderboard ranked by cumulative bond across all companions.
        /// </summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="pageSize">Items per page.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PagedResult<LeaderboardEntryDto>> GetTotalBondLeaderboardAsync(
            int page, int pageSize, CancellationToken ct);

        /// <summary>
        /// Returns a paged leaderboard ranked by bond with a specific companion.
        /// Only players who own the companion appear in the results.
        /// </summary>
        /// <param name="girlId">The global girl ID to scope the ranking to.</param>
        /// <param name="page">1-based page number.</param>
        /// <param name="pageSize">Items per page.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<PagedResult<LeaderboardEntryDto>> GetCompanionLeaderboardAsync(
            int girlId, int page, int pageSize, CancellationToken ct);

        /// <summary>
        /// Returns all companions that appear in at least one player's collection,
        /// ordered by name, for use in the companion selector dropdown.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<IReadOnlyList<CompanionOptionDto>> GetCompanionOptionsAsync(CancellationToken ct);
    }
}
