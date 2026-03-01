namespace EverydayGirlsCompanionCollector.Models.Enums
{
    /// <summary>
    /// Identifies which leaderboard to display.
    /// </summary>
    public enum LeaderboardType
    {
        /// <summary>
        /// Rank players by their cumulative bond across all companions.
        /// </summary>
        TotalBond = 0,

        /// <summary>
        /// Rank players by their bond with a single selected companion.
        /// </summary>
        CompanionBond = 1
    }
}
