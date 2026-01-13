using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Implementation of daily state service.
    /// Reset time is 18:00 UTC.
    /// </summary>
    public class DailyStateService : IDailyStateService
    {
        private static readonly TimeOnly ResetTime = new TimeOnly(18, 0); // 18:00 UTC

        /// <summary>
        /// Gets the current ServerDate based on 18:00 UTC reset time.
        /// If current UTC time is before 18:00, ServerDate is yesterday.
        /// If current UTC time is at or after 18:00, ServerDate is today.
        /// </summary>
        public DateOnly GetCurrentServerDate()
        {
            var nowUtc = DateTime.UtcNow;
            var currentTime = TimeOnly.FromDateTime(nowUtc);

            if (currentTime >= ResetTime)
            {
                // After reset time, ServerDate is today
                return DateOnly.FromDateTime(nowUtc);
            }
            else
            {
                // Before reset time, ServerDate is yesterday
                return DateOnly.FromDateTime(nowUtc.AddDays(-1));
            }
        }

        /// <summary>
        /// Calculates time remaining until the next 18:00 UTC reset.
        /// </summary>
        public TimeSpan GetTimeUntilReset()
        {
            var nowUtc = DateTime.UtcNow;
            var currentTime = TimeOnly.FromDateTime(nowUtc);
            var today = DateOnly.FromDateTime(nowUtc);

            DateTime nextReset;

            if (currentTime >= ResetTime)
            {
                // Next reset is tomorrow at 18:00 UTC
                nextReset = today.AddDays(1).ToDateTime(ResetTime);
            }
            else
            {
                // Next reset is today at 18:00 UTC
                nextReset = today.ToDateTime(ResetTime);
            }

            return nextReset - nowUtc;
        }

        /// <summary>
        /// Checks if Daily Roll is available (LastDailyRollDate != ServerDate).
        /// </summary>
        public bool IsDailyRollAvailable(UserDailyState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            var serverDate = GetCurrentServerDate();
            return state.LastDailyRollDate != serverDate;
        }

        /// <summary>
        /// Checks if Daily Adopt is available (LastDailyAdoptDate != ServerDate).
        /// </summary>
        public bool IsDailyAdoptAvailable(UserDailyState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            var serverDate = GetCurrentServerDate();
            return state.LastDailyAdoptDate != serverDate;
        }

        /// <summary>
        /// Checks if Daily Interaction is available (LastDailyInteractionDate != ServerDate).
        /// </summary>
        public bool IsDailyInteractionAvailable(UserDailyState state)
        {
            ArgumentNullException.ThrowIfNull(state);
            var serverDate = GetCurrentServerDate();
            return state.LastDailyInteractionDate != serverDate;
        }
    }
}
