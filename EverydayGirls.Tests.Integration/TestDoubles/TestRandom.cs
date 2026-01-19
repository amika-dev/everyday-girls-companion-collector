using EverydayGirlsCompanionCollector.Abstractions;

namespace EverydayGirls.Tests.Integration.TestDoubles
{
    /// <summary>
    /// Test double for IRandom providing deterministic, scriptable random values.
    /// </summary>
    /// <remarks>
    /// Supports two modes:
    /// 1. Sequential mode: Returns values from a predefined sequence
    /// 2. Fixed mode: Returns a single fixed value repeatedly
    /// 
    /// Used to test randomness-dependent behaviors deterministically:
    /// - Bond calculation (+1 vs +2)
    /// - Candidate shuffling
    /// 
    /// Thread-safe for concurrent test execution.
    /// </remarks>
    public sealed class TestRandom : IRandom
    {
        private readonly object _lock = new object();
        private readonly Queue<int> _values = new Queue<int>();
        private int? _fixedValue;

        /// <summary>
        /// Initializes a TestRandom with a sequence of values to return.
        /// </summary>
        /// <param name="values">The sequence of values to return from Next(). Cycles when exhausted.</param>
        public TestRandom(params int[] values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Length == 0)
            {
                throw new ArgumentException("Must provide at least one value", nameof(values));
            }

            foreach (var value in values)
            {
                _values.Enqueue(value);
            }
        }

        /// <summary>
        /// Sets a fixed value to return from all Next() calls.
        /// </summary>
        public void SetFixedValue(int value)
        {
            lock (_lock)
            {
                _fixedValue = value;
            }
        }

        /// <summary>
        /// Clears the fixed value and resumes sequential mode.
        /// </summary>
        public void ClearFixedValue()
        {
            lock (_lock)
            {
                _fixedValue = null;
            }
        }

        /// <summary>
        /// Returns the next value from the sequence (or fixed value if set).
        /// </summary>
        /// <param name="maxValue">Maximum value (exclusive). Returned value is modulo this.</param>
        public int Next(int maxValue)
        {
            lock (_lock)
            {
                if (_fixedValue.HasValue)
                {
                    return _fixedValue.Value % maxValue;
                }

                // Get next value from queue and re-enqueue it (cycle)
                var value = _values.Dequeue();
                _values.Enqueue(value);

                return value % maxValue;
            }
        }

        /// <summary>
        /// Shuffles an array using a deterministic sequence.
        /// </summary>
        /// <remarks>
        /// Uses Fisher-Yates shuffle with controlled random values.
        /// For testing, you can script the shuffle by providing a specific sequence.
        /// </remarks>
        public void Shuffle<T>(T[] array)
        {
            ArgumentNullException.ThrowIfNull(array);

            lock (_lock)
            {
                // Fisher-Yates shuffle with controlled randomness
                for (int i = array.Length - 1; i > 0; i--)
                {
                    int j = Next(i + 1);
                    (array[j], array[i]) = (array[i], array[j]);
                }
            }
        }
    }
}



