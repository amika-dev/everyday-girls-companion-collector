using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Read-only list item for a companion in a friend's collection.
    /// Mirrors the user's own collection grid display fields without any action flags.
    /// </summary>
    public record FriendGirlListItemDto
    {
        /// <summary>
        /// The girl's unique identifier.
        /// </summary>
        public int GirlId { get; init; }

        /// <summary>
        /// The girl's display name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// URL or path to the girl's portrait image.
        /// </summary>
        public required string ImageUrl { get; init; }

        /// <summary>
        /// Current bond value between the friend and this companion.
        /// </summary>
        public int Bond { get; init; }

        /// <summary>
        /// The personality tag assigned to this companion.
        /// </summary>
        public PersonalityTag PersonalityTag { get; init; }

        /// <summary>
        /// Number of ServerDate transitions since the friend adopted this companion.
        /// </summary>
        public int DaysTogether { get; init; }

        /// <summary>
        /// True if this companion is the friend's current partner.
        /// </summary>
        public bool IsPartner { get; init; }
    }
}
