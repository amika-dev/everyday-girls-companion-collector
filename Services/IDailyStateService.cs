using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for calculating daily state (ServerDate, reset time, availability checks).
    /// </summary>
    public interface IDailyStateService
    {
        /// <summary>
        /// Gets the current ServerDate based on 18:00 UTC reset time.
        /// If current time is before 18:00 UTC, returns yesterday. Otherwise returns today.
        /// </summary>
        DateOnly GetCurrentServerDate();

        /// <summary>
        /// Calculates time remaining until the next daily reset (18:00 UTC).
        /// </summary>
        TimeSpan GetTimeUntilReset();

        /// <summary>
        /// Checks if Daily Roll is available for the current ServerDate.
        /// </summary>
        bool IsDailyRollAvailable(UserDailyState state);

        /// <summary>
        /// Checks if Daily Adopt is available for the current ServerDate.
        /// </summary>
        bool IsDailyAdoptAvailable(UserDailyState state);

        /// <summary>
        /// Checks if Daily Interaction is available for the current ServerDate.
        /// </summary>
        bool IsDailyInteractionAvailable(UserDailyState state);
    }
}
