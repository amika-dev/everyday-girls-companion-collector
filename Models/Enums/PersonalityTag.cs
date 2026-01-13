namespace EverydayGirlsCompanionCollector.Models.Enums
{
    /// <summary>
    /// Personality tags that affect dialogue pool only.
    /// Default tag is the first enum value (Cheerful).
    /// </summary>
    public enum PersonalityTag
    {
        /// <summary>
        /// Cheerful personality - happy and optimistic dialogue
        /// </summary>
        Cheerful = 0,

        /// <summary>
        /// Shy personality - quiet and reserved dialogue
        /// </summary>
        Shy = 1,

        /// <summary>
        /// Energetic personality - lively and enthusiastic dialogue
        /// </summary>
        Energetic = 2,

        /// <summary>
        /// Calm personality - peaceful and relaxed dialogue
        /// </summary>
        Calm = 3,

        /// <summary>
        /// Playful personality - fun and teasing dialogue
        /// </summary>
        Playful = 4
    }
}
