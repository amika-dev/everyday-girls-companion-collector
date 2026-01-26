namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Tracks which locked town locations a user has unlocked.
    /// </summary>
    public class UserTownLocationUnlock
    {
        /// <summary>
        /// User ID (composite key part 1).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Town location ID (composite key part 2).
        /// </summary>
        public int TownLocationId { get; set; }

        /// <summary>
        /// UTC timestamp when the location was unlocked.
        /// </summary>
        public DateTime UnlockedUtc { get; set; }

        /// <summary>
        /// Navigation property to the user.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Navigation property to the town location.
        /// </summary>
        public TownLocation TownLocation { get; set; } = null!;
    }
}
