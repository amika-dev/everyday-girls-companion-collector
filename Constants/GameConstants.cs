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

        /// <summary>
        /// Number of companions shown per page in the collection grid (2 rows × 5 columns).
        /// </summary>
        public const int CollectionPageSize = 10;

        /// <summary>
        /// Default number of items per page for friends list and user search results.
        /// </summary>
        public const int FriendsPageSize = 5;
    }
}
