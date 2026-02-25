using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for reading profile summaries and managing display names.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Retrieves the profile summary for the given user.
        /// </summary>
        /// <param name="userId">The Identity user ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A populated <see cref="ProfileViewModel"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user is not found.</exception>
        Task<ProfileViewModel> GetProfileAsync(string userId, CancellationToken ct = default);

        /// <summary>
        /// Attempts to change the user's display name, enforcing all business rules.
        /// Rules checked in order: format, unchanged, once-per-reset, uniqueness.
        /// </summary>
        /// <param name="userId">The Identity user ID.</param>
        /// <param name="newDisplayName">The requested new display name.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="DisplayNameChangeResult"/> indicating success or the reason for failure.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user is not found.</exception>
        Task<DisplayNameChangeResult> TryChangeDisplayNameAsync(string userId, string newDisplayName, CancellationToken ct = default);
    }
}
