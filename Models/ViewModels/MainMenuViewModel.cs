using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for the Main Menu (Hub) screen.
    /// Displays daily status indicators and partner information.
    /// </summary>
    public class MainMenuViewModel
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
        /// Whether Daily Interaction is available.
        /// </summary>
        public bool IsDailyInteractionAvailable { get; set; }

        /// <summary>
        /// Time remaining until the next daily reset.
        /// </summary>
        public TimeSpan TimeUntilReset { get; set; }

        /// <summary>
        /// Current partner girl. Null if user has never adopted.
        /// </summary>
        public Girl? Partner { get; set; }

        /// <summary>
        /// Partner's current bond value. Null if no partner.
        /// </summary>
        public int? PartnerBond { get; set; }
    }
}
