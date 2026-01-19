namespace EverydayGirlsCompanionCollector.Models
{
    /// <summary>
    /// Represents a gameplay tip or hint displayed to users.
    /// </summary>
    public record GameplayTip
    {
        /// <summary>
        /// Unique identifier for the tip.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Short title for the tip (used in Guide display).
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Tip body text.
        /// </summary>
        public required string Body { get; init; }

        /// <summary>
        /// Optional emoji icon for visual warmth.
        /// </summary>
        public string? Icon { get; init; }
    }
}
