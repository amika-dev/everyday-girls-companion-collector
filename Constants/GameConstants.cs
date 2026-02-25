namespace EverydayGirlsCompanionCollector.Constants
{
    /// <summary>
    /// Application-wide constants for game rules and limits.
    /// </summary>
    public static class GameConstants
    {
        /// <summary>
        /// Maximum number of girls a user can own in their collection.
        /// </summary>
        public const int MaxCollectionSize = 30;

        /// <summary>
        /// Number of candidate girls presented in each Daily Roll.
        /// </summary>
        public const int DailyCandidateCount = 5;

        /// <summary>
        /// Hour (in UTC) when daily actions reset (18:00 UTC).
        /// </summary>
        public const int DailyResetHourUtc = 18;

        /// <summary>
        /// Minimum character length for a display name.
        /// </summary>
        public const int DisplayNameMinLength = 4;

        /// <summary>
        /// Maximum character length for a display name.
        /// </summary>
        public const int DisplayNameMaxLength = 16;
    }
}
