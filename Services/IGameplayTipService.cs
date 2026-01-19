using EverydayGirlsCompanionCollector.Models;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for retrieving gameplay tips and hints.
    /// </summary>
    public interface IGameplayTipService
    {
        /// <summary>
        /// Gets all available gameplay tips.
        /// </summary>
        /// <returns>Read-only list of all tips.</returns>
        IReadOnlyList<GameplayTip> GetAllTips();

        /// <summary>
        /// Gets a random gameplay tip.
        /// </summary>
        /// <returns>A randomly selected tip.</returns>
        GameplayTip GetRandomTip();
    }
}
