using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Query parameters for leaderboard pages, bound from the request query string.
    /// </summary>
    public class LeaderboardFilterDto
    {
        /// <summary>
        /// Which leaderboard to display. Defaults to TotalBond.
        /// </summary>
        public LeaderboardType Type { get; set; } = LeaderboardType.TotalBond;

        /// <summary>
        /// The companion's girl ID to scope the leaderboard to, if on the companion board.
        /// </summary>
        public int? GirlId { get; set; }

        /// <summary>
        /// 1-based page number. Defaults to 1.
        /// </summary>
        public int Page { get; set; } = 1;
    }
}
