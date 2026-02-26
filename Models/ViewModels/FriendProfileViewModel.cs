using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Read-only view model for viewing a friend's profile.
    /// Contains the same display fields as the user's own profile page, without any edit flags.
    /// </summary>
    public record FriendProfileViewModel
    {
        /// <summary>
        /// The friend's user ID.
        /// </summary>
        public required string FriendUserId { get; init; }

        /// <summary>
        /// The friend's display name.
        /// </summary>
        public required string DisplayName { get; init; }

        /// <summary>
        /// The friend's partner companion name. Null if no partner.
        /// </summary>
        public string? PartnerName { get; init; }

        /// <summary>
        /// URL or path to the friend's partner companion image. Null if no partner.
        /// </summary>
        public string? PartnerImagePath { get; init; }

        /// <summary>
        /// The friend's bond level with their partner. Null if no partner.
        /// </summary>
        public int? PartnerBond { get; init; }

        /// <summary>
        /// UTC timestamp of when the friend's partner was first met. Null if no partner.
        /// </summary>
        public DateTime? PartnerFirstMetUtc { get; init; }

        /// <summary>
        /// Number of ServerDate transitions since the friend adopted their partner. Null if no partner.
        /// </summary>
        public int? PartnerDaysTogether { get; init; }

        /// <summary>
        /// The friend's partner personality tag. Null if no partner.
        /// </summary>
        public PersonalityTag? PartnerPersonalityTag { get; init; }

        /// <summary>
        /// Total number of companions the friend has collected.
        /// </summary>
        public int CompanionsCount { get; init; }

        /// <summary>
        /// Total bond across all companions the friend owns.
        /// </summary>
        public int TotalBond { get; init; }
    }
}
