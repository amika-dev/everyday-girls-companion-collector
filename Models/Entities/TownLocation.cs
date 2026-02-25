using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Represents a location in town where companions can be assigned for daily activities.
    /// Configuration data - seeded at startup.
    /// </summary>
    public class TownLocation
    {
        /// <summary>
        /// Primary key (identity).
        /// </summary>
        public int TownLocationId { get; set; }

        /// <summary>
        /// Display name of the location.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The primary skill trained at this location.
        /// </summary>
        public SkillType PrimarySkill { get; set; }

        /// <summary>
        /// Base bond points gained daily when a companion is assigned here.
        /// </summary>
        public int BaseDailyBondGain { get; set; } = 1;

        /// <summary>
        /// Base currency earned daily when a companion is assigned here.
        /// </summary>
        public int BaseDailyCurrencyGain { get; set; } = 5;

        /// <summary>
        /// Base skill points gained daily in the primary skill.
        /// </summary>
        public int BaseDailySkillGain { get; set; } = 10;

        /// <summary>
        /// Whether this location requires unlocking before use.
        /// </summary>
        public bool IsLockedByDefault { get; set; }

        /// <summary>
        /// Currency cost to unlock this location (if locked by default).
        /// </summary>
        public int UnlockCost { get; set; } = 50;
    }
}
