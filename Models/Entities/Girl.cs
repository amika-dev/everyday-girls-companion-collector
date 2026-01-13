namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Represents a girl in the global pool available for adoption.
    /// </summary>
    public class Girl
    {
        /// <summary>
        /// Unique identifier for the girl.
        /// </summary>
        public int GirlId { get; set; }

        /// <summary>
        /// Girl's display name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL or path to the girl's image.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
