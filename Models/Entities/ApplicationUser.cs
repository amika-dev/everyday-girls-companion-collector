using Microsoft.AspNetCore.Identity;

namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Application user extending ASP.NET Core Identity with partner tracking and profile data.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Display name chosen by the user (4-16 alphanumeric characters).
        /// </summary>
        public string DisplayName { get; set; } = "Townie";

        /// <summary>
        /// Uppercase normalized version of DisplayName for case-insensitive lookups.
        /// </summary>
        public string DisplayNameNormalized { get; set; } = "TOWNIE";

        /// <summary>
        /// UTC timestamp of the last display name change. Null if never changed.
        /// </summary>
        public DateTime? LastDisplayNameChangeUtc { get; set; }

        /// <summary>
        /// Player's current currency balance for purchasing unlocks.
        /// </summary>
        public int CurrencyBalance { get; set; }

        /// <summary>
        /// Foreign key to the current partner girl. Nullable - first adoption sets this automatically.
        /// </summary>
        public int? PartnerGirlId { get; set; }

        /// <summary>
        /// Navigation property to the partner girl.
        /// </summary>
        public Girl? Partner { get; set; }

        /// <summary>
        /// Navigation property to friend relationships where this user is the requester.
        /// </summary>
        public ICollection<FriendRelationship> Friends { get; set; } = new List<FriendRelationship>();

        /// <summary>
        /// Navigation property to friend relationships where this user is the friend.
        /// </summary>
        public ICollection<FriendRelationship> FriendOf { get; set; } = new List<FriendRelationship>();
    }
}
