namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// A single row on any leaderboard, projected from the database.
    /// </summary>
    public record LeaderboardEntryDto
    {
        /// <summary>
        /// 1-based rank position on the leaderboard.
        /// </summary>
        public required int Rank { get; init; }

        /// <summary>
        /// The ranked player's user ID.
        /// </summary>
        public required string UserId { get; init; }

        /// <summary>
        /// The ranked player's chosen display name.
        /// </summary>
        public required string DisplayName { get; init; }

        /// <summary>
        /// Image path for the player's current partner companion, if any.
        /// </summary>
        public string? PartnerImageUrl { get; init; }

        /// <summary>
        /// Name of the player's current partner companion, if any.
        /// </summary>
        public string? PartnerName { get; init; }

        /// <summary>
        /// Cumulative bond across all companions (used for the player-level board).
        /// </summary>
        public required int TotalBond { get; init; }

        /// <summary>
        /// Bond with the selected companion (used for companion-specific boards; null on player boards).
        /// </summary>
        public int? BondWithSelectedCompanion { get; init; }
    }
}
