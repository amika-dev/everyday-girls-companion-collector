namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Result of a display name change attempt.
    /// Use <see cref="Success"/> and <see cref="Failure"/> factory methods to construct.
    /// </summary>
    public record DisplayNameChangeResult
    {
        /// <summary>
        /// True if the display name was changed successfully.
        /// </summary>
        public bool Succeeded { get; init; }

        /// <summary>
        /// Human-readable reason for failure. Null when <see cref="Succeeded"/> is true.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static DisplayNameChangeResult Success() => new() { Succeeded = true };

        /// <summary>
        /// Creates a failed result with a reason.
        /// </summary>
        public static DisplayNameChangeResult Failure(string errorMessage) =>
            new() { Succeeded = false, ErrorMessage = errorMessage };
    }
}
