namespace EverydayGirlsCompanionCollector.Abstractions
{
    /// <summary>
    /// System implementation of IClock that returns the actual current time.
    /// </summary>
    public class SystemClock : IClock
    {
        /// <inheritdoc />
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
