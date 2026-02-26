using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles the friends list, user search, and adding friends.
    /// </summary>
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendsQuery _friendsQuery;
        private readonly IFriendsService _friendsService;

        public FriendsController(IFriendsQuery friendsQuery, IFriendsService friendsService)
        {
            ArgumentNullException.ThrowIfNull(friendsQuery);
            ArgumentNullException.ThrowIfNull(friendsService);
            _friendsQuery = friendsQuery;
            _friendsService = friendsService;
        }

        /// <summary>
        /// Displays the user's friends list.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var friends = await _friendsQuery.GetFriendsAsync(userId, ct);
            return View(friends);
        }

        /// <summary>
        /// Displays the add-friends search page with optional search results.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Add(string? q, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["SearchQuery"] = q;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var results = await _friendsQuery.SearchUsersByDisplayNameAsync(userId, q, 25, ct);
                return View(results);
            }

            return View();
        }

        /// <summary>
        /// Adds a friend and redirects following the PRG pattern.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string friendUserId, string? q, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(friendUserId))
            {
                TempData["ErrorMessage"] = "Something went wrong — please try again.";
                return RedirectToAction(nameof(Add), new { q });
            }

            var result = await _friendsService.TryAddFriendAsync(userId, friendUserId, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Friend added!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not add friend — please try again.";
            return RedirectToAction(nameof(Add), new { q });
        }
    }
}
