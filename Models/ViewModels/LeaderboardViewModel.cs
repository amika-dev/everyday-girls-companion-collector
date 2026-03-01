using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for all leaderboard screens (player-level and companion-specific).
    /// </summary>
    public class LeaderboardViewModel
    {
        /// <summary>
        /// Page heading shown to the player (e.g. "Top Bond").
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Contextual subtitle beneath the heading (e.g. "Who's bonded the most?").
        /// </summary>
        public required string Subtitle { get; set; }

        /// <summary>
        /// Identifies which leaderboard type is being displayed.
        /// </summary>
        public required LeaderboardType Type { get; set; }

        /// <summary>
        /// The active filter values parsed from the request.
        /// </summary>
        public required LeaderboardFilterDto Filter { get; set; }

        /// <summary>
        /// The ranked entries for the current page.
        /// </summary>
        public required PagedResult<LeaderboardEntryDto> Results { get; set; }

        /// <summary>
        /// Available companions for the companion selector dropdown.
        /// Empty when displaying a player-level board.
        /// </summary>
        public required IReadOnlyList<CompanionOptionDto> Companions { get; set; }
    }
}
