using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for managing daily roll candidate generation.
    /// </summary>
    public interface IDailyRollService
    {
        /// <summary>
        /// Generates random candidates from available girls (not owned by user).
        /// </summary>
        /// <param name="availableGirls">Girls not owned by the user.</param>
        /// <param name="count">Number of candidates to generate.</param>
        /// <returns>List of candidate girls.</returns>
        List<Girl> GenerateCandidates(Girl[] availableGirls, int count);
    }
}
