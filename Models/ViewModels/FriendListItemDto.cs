namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Represents a single friend in the user's friend list.
    /// </summary>
    public record FriendListItemDto
    {
        /// <summary>
        /// The friend's user ID.
        /// </summary>
        public required string UserId { get; init; }

        /// <summary>
        /// The friend's display name.
        /// </summary>
        public required string DisplayName { get; init; }

        /// <summary>
        /// The friend's partner companion name, if they have one.
        /// </summary>
        public string? PartnerName { get; init; }

        /// <summary>
        /// The friend's partner companion image path, if they have one.
        /// </summary>
        public string? PartnerImagePath { get; init; }

        /// <summary>
        /// The friend's bond level with their partner, if they have one.
        /// </summary>
        public int? PartnerBond { get; init; }

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
