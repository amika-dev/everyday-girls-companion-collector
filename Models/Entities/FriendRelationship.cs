namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Represents a one-way friend relationship between two users.
    /// Bidirectional friendships are represented by two rows with reversed UserId/FriendUserId.
    /// </summary>
    public class FriendRelationship
    {
        /// <summary>
        /// User who added the friend (composite key part 1).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User who was added as friend (composite key part 2).
        /// </summary>
        public string FriendUserId { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the friendship was established.
        /// </summary>
        public DateTime DateAddedUtc { get; set; }

        /// <summary>
        /// Navigation property to the user who initiated the friendship.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Navigation property to the friend user.
        /// </summary>
        public ApplicationUser Friend { get; set; } = null!;
    }
}
