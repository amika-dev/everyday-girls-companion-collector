using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Represents ownership and relationship data for a user's adopted girl.
    /// Uses a composite primary key (UserId + GirlId).
    /// </summary>
    public class UserGirl
    {
        /// <summary>
        /// User ID (composite key part 1).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Girl ID (composite key part 2).
        /// </summary>
        public int GirlId { get; set; }

        /// <summary>
        /// Date the girl was adopted (UTC).
        /// </summary>
        public DateTime DateMetUtc { get; set; }

        /// <summary>
        /// Current bond value. Starts at 0, increases through daily interactions.
        /// </summary>
        public int Bond { get; set; }

        /// <summary>
        /// Current personality tag assigned by user. Affects dialogue pool only.
        /// </summary>
        public PersonalityTag PersonalityTag { get; set; }

        /// <summary>
        /// Navigation property to the owning user.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Navigation property to the girl entity.
        /// </summary>
        public Girl Girl { get; set; } = null!;
    }
}
