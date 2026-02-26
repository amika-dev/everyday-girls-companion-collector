using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// Read-only detail view for a single companion in a friend's collection.
    /// Used by the immutable "More About Her" modal. Contains no action flags or edit options.
    /// </summary>
    public record FriendGirlDetailsDto
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
        /// UTC timestamp of when the friend first met (adopted) this companion.
        /// </summary>
        public DateTime DateMetUtc { get; init; }

        /// <summary>
        /// Number of ServerDate transitions since the friend adopted this companion.
        /// </summary>
        public int DaysTogether { get; init; }

        /// <summary>
        /// True if this companion is the friend's current partner.
        /// </summary>
        public bool IsPartner { get; init; }

        /// <summary>
        /// Charm skill value for this companion.
        /// </summary>
        public int Charm { get; init; }

        /// <summary>
        /// Focus skill value for this companion.
        /// </summary>
        public int Focus { get; init; }

        /// <summary>
        /// Vitality skill value for this companion.
        /// </summary>
        public int Vitality { get; init; }
    }
}
