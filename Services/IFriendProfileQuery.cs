using EverydayGirlsCompanionCollector.Models.ViewModels;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query for viewing a friend's profile data.
    /// </summary>
    public interface IFriendProfileQuery
    {
        /// <summary>
        /// Returns the read-only profile data for a friend user.
        /// Returns null if the friend user does not exist.
        /// </summary>
        /// <param name="viewerUserId">The viewing user's ID (not used for authorization, reserved for future use).</param>
        /// <param name="friendUserId">The friend whose profile to retrieve.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<FriendProfileViewModel?> GetFriendProfileAsync(string viewerUserId, string friendUserId, CancellationToken ct);
    }
}
