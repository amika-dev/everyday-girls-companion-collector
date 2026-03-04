using EverydayGirlsCompanionCollector.Models.Enums;

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

        /// <summary>
        /// Default personality tag for this girl. When set, newly created UserGirl records will
        /// inherit this value instead of falling back to the enum default.
        /// </summary>
        public PersonalityTag? DefaultPersonalityTag { get; set; }
    }
}
