using EverydayGirlsCompanionCollector.Constants;

namespace EverydayGirlsCompanionCollector.Utilities
{
    /// <summary>
    /// Helper for computing values based on the app's daily reset cadence (18:00 UTC).
    /// </summary>
    public static class DailyCadence
    {
        /// <summary>
        /// Converts a UTC timestamp to its corresponding ServerDate based on the 18:00 UTC reset rule.
        /// </summary>
        public static DateOnly GetServerDateFromUtc(DateTime utcTimestamp)
        {
            var currentTime = TimeOnly.FromDateTime(utcTimestamp);
            var resetTime = new TimeOnly(GameConstants.DailyResetHourUtc, 0);

            return currentTime >= resetTime
                ? DateOnly.FromDateTime(utcTimestamp)
                : DateOnly.FromDateTime(utcTimestamp.AddDays(-1));
        }

        /// <summary>
        /// Computes the number of ServerDate transitions since adoption.
        /// Returns 0 if no reset has occurred since adoption.
        /// </summary>
        public static int GetDaysSinceAdoption(DateOnly currentServerDate, DateTime dateMetUtc)
        {
            var adoptionServerDate = GetServerDateFromUtc(dateMetUtc);
            return currentServerDate.DayNumber - adoptionServerDate.DayNumber;
        }
    }
}
