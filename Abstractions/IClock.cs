namespace EverydayGirlsCompanionCollector.Abstractions
{
    /// <summary>
    /// Abstraction for retrieving the current time.
    /// Allows time-based logic to be testable.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
