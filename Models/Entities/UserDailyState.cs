namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Tracks daily system availability and persisted candidates for a single user.
    /// One row per user. Created automatically on user registration.
    /// </summary>
    public class UserDailyState
    {
        /// <summary>
        /// User ID (primary key).
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Last date Daily Roll was used.
        /// </summary>
        public DateOnly? LastDailyRollDate { get; set; }

        /// <summary>
        /// Last date Daily Adopt was used.
        /// </summary>
        public DateOnly? LastDailyAdoptDate { get; set; }

        /// <summary>
        /// Last date Daily Interaction was used.
        /// </summary>
        public DateOnly? LastDailyInteractionDate { get; set; }

        /// <summary>
        /// Date when current candidates were generated. Must match ServerDate for candidates to be valid.
        /// </summary>
        public DateOnly? CandidateDate { get; set; }

        /// <summary>
        /// First candidate girl ID (nullable).
        /// </summary>
        public int? Candidate1GirlId { get; set; }

        /// <summary>
        /// Second candidate girl ID (nullable).
        /// </summary>
        public int? Candidate2GirlId { get; set; }

        /// <summary>
        /// Third candidate girl ID (nullable).
        /// </summary>
        public int? Candidate3GirlId { get; set; }

        /// <summary>
        /// Fourth candidate girl ID (nullable).
        /// </summary>
        public int? Candidate4GirlId { get; set; }

    /// <summary>
    /// Fifth candidate girl ID (nullable).
    /// </summary>
    public int? Candidate5GirlId { get; set; }

    /// <summary>
    /// The girl ID that was adopted today (nullable).
    /// Used to display personalized adoption message and visual indicator.
    /// </summary>
    public int? TodayAdoptedGirlId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
    }
}
