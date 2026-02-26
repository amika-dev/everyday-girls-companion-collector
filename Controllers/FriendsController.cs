using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles the friends list, user search, adding/removing friends, and viewing friend profiles and collections.
    /// </summary>
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendsQuery _friendsQuery;
        private readonly IFriendsService _friendsService;
        private readonly IFriendProfileQuery _friendProfileQuery;
        private readonly IFriendCollectionQuery _friendCollectionQuery;
        private readonly ApplicationDbContext _context;

        public FriendsController(
            IFriendsQuery friendsQuery,
            IFriendsService friendsService,
            IFriendProfileQuery friendProfileQuery,
            IFriendCollectionQuery friendCollectionQuery,
            ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(friendsQuery);
            ArgumentNullException.ThrowIfNull(friendsService);
            ArgumentNullException.ThrowIfNull(friendProfileQuery);
            ArgumentNullException.ThrowIfNull(friendCollectionQuery);
            ArgumentNullException.ThrowIfNull(context);
            _friendsQuery = friendsQuery;
            _friendsService = friendsService;
            _friendProfileQuery = friendProfileQuery;
            _friendCollectionQuery = friendCollectionQuery;
            _context = context;
        }

        /// <summary>
        /// Displays the user's friends list with paging.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var friends = await _friendsQuery.GetFriendsAsync(userId, page, GameConstants.FriendsPageSize, ct);
            return View(friends);
        }

        /// <summary>
        /// Displays the add-friends search page with optional paged search results.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Add(string? q, int page = 1, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["SearchQuery"] = q;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var results = await _friendsQuery.SearchUsersByDisplayNameAsync(userId, q, page, GameConstants.FriendsPageSize, ct);
                return View(results);
            }

            return View((PagedResult<UserSearchResultDto>?)null);
        }

        /// <summary>
        /// Adds a friend and redirects following the PRG pattern.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string friendUserId, string? q, int page = 1, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(friendUserId))
            {
                TempData["ErrorMessage"] = "Something went wrong — please try again.";
                return RedirectToAction(nameof(Add), new { q, page });
            }

            var result = await _friendsService.TryAddFriendAsync(userId, friendUserId, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Friend added!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not add friend — please try again.";
            return RedirectToAction(nameof(Add), new { q, page });
        }

        /// <summary>
        /// Displays a friend's read-only profile. Returns 404 if not friends or user not found.
        /// </summary>
        [HttpGet("/Friends/{friendUserId}/Profile")]
        public async Task<IActionResult> Profile(string friendUserId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!await IsFriendAsync(userId, friendUserId, ct))
            {
                return NotFound();
            }

            var profile = await _friendProfileQuery.GetFriendProfileAsync(userId, friendUserId, ct);
            if (profile is null)
            {
                return NotFound();
            }

            return View(profile);
        }

        /// <summary>
        /// Displays a friend's companion collection with paging. Returns 404 if not friends.
        /// </summary>
        [HttpGet("/Friends/{friendUserId}/Collection")]
        public async Task<IActionResult> Collection(string friendUserId, int page = 1, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!await IsFriendAsync(userId, friendUserId, ct))
            {
                return NotFound();
            }

            var collection = await _friendCollectionQuery.GetFriendCollectionAsync(userId, friendUserId, page, GameConstants.FriendsPageSize, ct);
            return View(collection);
        }

        /// <summary>
        /// Removes a friend and redirects following the PRG pattern. Returns 404 if not friends.
        /// </summary>
        [HttpPost("/Friends/{friendUserId}/Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string friendUserId, int page = 1, CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!await IsFriendAsync(userId, friendUserId, ct))
            {
                return NotFound();
            }

            var result = await _friendsService.TryRemoveFriendAsync(userId, friendUserId, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Friend removed.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not remove friend — please try again.";
            }

            return RedirectToAction(nameof(Index), new { page });
        }

        /// <summary>
        /// Returns true if a friendship record exists from viewerUserId to friendUserId.
        /// Does not leak whether the friend user exists — unknown user IDs simply return false.
        /// </summary>
        private Task<bool> IsFriendAsync(string viewerUserId, string friendUserId, CancellationToken ct) =>
            _context.FriendRelationships
                .AnyAsync(fr => fr.UserId == viewerUserId && fr.FriendUserId == friendUserId, ct);
    }
}
