using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for retrieving random dialogue lines based on personality tag.
    /// </summary>
    public interface IDialogueService
    {
        /// <summary>
        /// Gets a random dialogue line for the specified personality tag.
        /// Repeats are allowed.
        /// </summary>
        string GetRandomDialogue(PersonalityTag tag);
    }
}
