namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for the Profile page.
    /// Provides a personal summary of the user's identity and companion collection.
    /// </summary>
    public class ProfileViewModel
    {
        /// <summary>
        /// The user's current display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The current partner companion's name. Null if no partner has been assigned.
        /// </summary>
        public string? PartnerName { get; set; }

        /// <summary>
        /// URL or path to the current partner companion's image. Null if no partner.
        /// </summary>
        public string? PartnerImageUrl { get; set; }

        /// <summary>
        /// The current partner companion's bond value. Null if no partner.
        /// </summary>
        public int? PartnerBond { get; set; }

        /// <summary>
        /// Total bond across all companions in the user's collection.
        /// </summary>
        public int TotalBond { get; set; }

        /// <summary>
        /// Total number of companions the user has collected.
        /// </summary>
        public int TotalCompanions { get; set; }

        /// <summary>
        /// Whether the user is permitted to change their display name this ServerDate.
        /// False if they have already changed it since the last daily reset.
        /// </summary>
        public bool CanChangeDisplayName { get; set; }
    }
}
