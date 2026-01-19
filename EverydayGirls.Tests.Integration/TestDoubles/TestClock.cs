using EverydayGirlsCompanionCollector.Abstractions;

namespace EverydayGirls.Tests.Integration.TestDoubles
{
    /// <summary>
    /// Test double for IClock allowing deterministic time control in integration tests.
    /// </summary>
    /// <remarks>
    /// Enables setting a fixed UTC time for testing time-dependent behaviors like:
    /// - Server date calculation (18:00 UTC boundary)
    /// - Daily action availability
    /// - Time-until-reset calculations
    /// 
    /// Thread-safe for concurrent test execution.
    /// </remarks>
    public sealed class TestClock : IClock
    {
        private DateTime _utcNow;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new TestClock with the specified UTC time.
        /// </summary>
        /// <param name="utcNow">The fixed UTC time to return from UtcNow.</param>
        public TestClock(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Time must be UTC", nameof(utcNow));
            }
            _utcNow = utcNow;
        }

        /// <summary>
        /// Gets the current controlled UTC time.
        /// </summary>
        public DateTime UtcNow
        {
            get
            {
                lock (_lock)
                {
                    return _utcNow;
                }
            }
        }

        /// <summary>
        /// Sets a new UTC time for subsequent calls to UtcNow.
        /// </summary>
        /// <param name="utcNow">The new UTC time.</param>
        public void SetUtcNow(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Time must be UTC", nameof(utcNow));
            }

            lock (_lock)
            {
                _utcNow = utcNow;
            }
        }

        /// <summary>
        /// Advances time by the specified duration.
        /// </summary>
        public void Advance(TimeSpan duration)
        {
            lock (_lock)
            {
                _utcNow = _utcNow.Add(duration);
            }
        }
    }
}



