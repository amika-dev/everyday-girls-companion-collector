namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Result of a friend-add attempt.
    /// Use <see cref="CreateSuccess"/> and <see cref="CreateFailure"/> factory methods to construct.
    /// </summary>
    public record AddFriendResult
    {
        /// <summary>
        /// True if the friend was added successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Machine-readable error code. Null when <see cref="Success"/> is true.
        /// </summary>
        public string? ErrorCode { get; init; }

        /// <summary>
        /// Human-readable error message. Null when <see cref="Success"/> is true.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static AddFriendResult CreateSuccess() => new() { Success = true };

        /// <summary>
        /// Creates a failed result with a code and message.
        /// </summary>
        public static AddFriendResult CreateFailure(string code, string message) =>
            new() { Success = false, ErrorCode = code, ErrorMessage = message };
    }
}
