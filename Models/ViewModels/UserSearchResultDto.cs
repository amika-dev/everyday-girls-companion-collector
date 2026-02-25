namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Represents a user returned from a display name search.
    /// </summary>
    public record UserSearchResultDto
    {
        /// <summary>
        /// The user's ID.
        /// </summary>
        public required string UserId { get; init; }

        /// <summary>
        /// The user's display name.
        /// </summary>
        public required string DisplayName { get; init; }

        /// <summary>
        /// The user's partner companion image path, if they have one.
        /// </summary>
        public string? PartnerImagePath { get; init; }

        /// <summary>
        /// True if the requester is already friends with this user.
        /// </summary>
        public bool IsAlreadyFriend { get; init; }

        /// <summary>
        /// True if the requester can send a friend request to this user (not self and not already friends).
        /// </summary>
        public bool CanAdd { get; init; }
    }
}
