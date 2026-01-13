using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for the Daily Roll + Adopt unified screen.
    /// </summary>
    public class DailyAdoptViewModel
    {
        /// <summary>
        /// Whether Daily Roll is available.
        /// </summary>
        public bool IsDailyRollAvailable { get; set; }

        /// <summary>
        /// Whether Daily Adopt is available.
        /// </summary>
        public bool IsDailyAdoptAvailable { get; set; }

        /// <summary>
        /// Today's candidate girls (empty if Daily Roll not used yet).
        /// </summary>
        public List<Girl> Candidates { get; set; } = new();

        /// <summary>
        /// Time remaining until next reset.
        /// </summary>
        public TimeSpan TimeUntilReset { get; set; }

        /// <summary>
        /// Current count of owned girls.
        /// </summary>
        public int OwnedGirlsCount { get; set; }

        /// <summary>
        /// Whether the user has reached the collection cap (100 girls).
        /// </summary>
        public bool IsCollectionFull => OwnedGirlsCount >= 100;
    }
}
