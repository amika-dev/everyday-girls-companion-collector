namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// A companion option for the leaderboard companion selector dropdown.
    /// </summary>
    public record CompanionOptionDto
    {
        /// <summary>
        /// The companion's global girl ID.
        /// </summary>
        public required int GirlId { get; init; }

        /// <summary>
        /// The companion's display name.
        /// </summary>
        public required string Name { get; init; }
    }
}
