namespace EverydayGirlsCompanionCollector.Abstractions
{
    /// <summary>
    /// System implementation of IRandom that uses Random.Shared.
    /// </summary>
    public class SystemRandom : IRandom
    {
        /// <inheritdoc />
        public int Next(int maxValue) => Random.Shared.Next(maxValue);

        /// <inheritdoc />
        public void Shuffle<T>(T[] values) => Random.Shared.Shuffle(values);
    }
}
