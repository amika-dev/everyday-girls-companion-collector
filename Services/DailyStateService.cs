using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Implementation of daily state service.
    /// Reset time is 18:00 UTC.
    /// </summary>
    public class DailyStateService : IDailyStateService
    {
        private static readonly TimeOnly ResetTime = new TimeOnly(GameConstants.DailyResetHourUtc, 0);

        /// <summary>
        /// Gets the current ServerDate based on 18:00 UTC reset time.
        /// If current UTC time is before 18:00, ServerDate is yesterday.
        /// If current UTC time is at or after 18:00, ServerDate is today.
        /// </summary>
        public DateOnly GetCurrentServerDate()
        {
            var nowUtc = DateTime.UtcNow;
            var currentTime = TimeOnly.FromDateTime(nowUtc);

            // After reset time (18:00 UTC), ServerDate is today; before reset time, ServerDate is yesterday
            return currentTime >= ResetTime 
                ? DateOnly.FromDateTime(nowUtc) 
                : DateOnly.FromDateTime(nowUtc.AddDays(-1));
        }

        /// <summary>
        /// Calculates time remaining until the next 18:00 UTC reset.
        /// </summary>
        public TimeSpan GetTimeUntilReset()
        {
            var nowUtc = DateTime.UtcNow;
            var currentTime = TimeOnly.FromDateTime(nowUtc);
            var today = DateOnly.FromDateTime(nowUtc);

            var nextReset = currentTime >= ResetTime
                ? today.AddDays(1).ToDateTime(ResetTime)   // Next reset is tomorrow at 18:00 UTC
                : today.ToDateTime(ResetTime);             // Next reset is today at 18:00 UTC
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
