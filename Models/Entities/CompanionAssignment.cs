namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Represents a companion's current assignment to a town location.
    /// Each companion can only be assigned to one location at a time.
    /// </summary>
    public class CompanionAssignment
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
        /// The town location where the companion is assigned.
        /// </summary>
        public int TownLocationId { get; set; }

        /// <summary>
        /// UTC timestamp when the companion was assigned to this location.
        /// </summary>
        public DateTime AssignedUtc { get; set; }

        /// <summary>
        /// Navigation property to the user-girl ownership record.
        /// </summary>
        public UserGirl UserGirl { get; set; } = null!;

        /// <summary>
        /// Navigation property to the town location.
        /// </summary>
        public TownLocation TownLocation { get; set; } = null!;
    }
}
