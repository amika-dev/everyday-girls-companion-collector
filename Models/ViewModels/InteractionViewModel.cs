using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for the Interaction screen.
    /// </summary>
    public class InteractionViewModel
    {
        /// <summary>
        /// Current partner girl.
        /// </summary>
        public Girl Partner { get; set; } = null!;

        /// <summary>
        /// Partner's current bond value.
        /// </summary>
        public int PartnerBond { get; set; }

        /// <summary>
        /// Partner's personality tag.
        /// </summary>
        public PersonalityTag PartnerTag { get; set; }

        /// <summary>
        /// Whether Daily Interaction is available.
        /// </summary>
        public bool IsDailyInteractionAvailable { get; set; }

        /// <summary>
        /// Time until next reset.
        /// </summary>
        public TimeSpan TimeUntilReset { get; set; }

        /// <summary>
        /// Dialogue line shown after interaction (from TempData).
        /// </summary>
        public string? Dialogue { get; set; }

        /// <summary>
        /// Whether the interaction resulted in +2 bond (rare occurrence).
        /// </summary>
        public bool WasSpecialMoment { get; set; }
    }
}
