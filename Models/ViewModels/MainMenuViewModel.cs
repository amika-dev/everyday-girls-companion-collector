using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;

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

        /// <summary>
        /// Date the partner was met. Null if no partner.
        /// </summary>
        public DateTime? PartnerDateMet { get; set; }

        /// <summary>
        /// Partner's personality tag. Null if no partner.
        /// </summary>
        public PersonalityTag? PartnerTag { get; set; }

        /// <summary>
        /// Days since partner was adopted (Days Together). Null if no partner.
        /// </summary>
        public int? PartnerDaysSinceAdoption => PartnerDateMet.HasValue
            ? (int)(DateTime.UtcNow - PartnerDateMet.Value).TotalDays
            : null;
    }
}
