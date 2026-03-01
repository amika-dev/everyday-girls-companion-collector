using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for a single leaderboard row partial.
    /// Carries the entry data and the board type so the partial can display the correct bond value.
    /// </summary>
    public record LeaderboardRowViewModel
    {
        public required LeaderboardEntryDto Entry { get; init; }
        public required LeaderboardType Type { get; init; }
    }
}
