namespace EverydayGirlsCompanionCollector.Abstractions
{
    /// <summary>
    /// Abstraction for random number generation.
    /// Allows random-based logic to be testable.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than maxValue.</returns>
        int Next(int maxValue);

        /// <summary>
        /// Shuffles the elements of an array in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="values">The array to shuffle.</param>
        void Shuffle<T>(T[] values);
    }
}
