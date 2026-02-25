using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Displays the user's profile summary and handles display name changes.
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            ArgumentNullException.ThrowIfNull(profileService);
            _profileService = profileService;
        }

        /// <summary>
        /// Shows the profile page for the logged-in user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _profileService.GetProfileAsync(userId, ct);
            return View(profile);
        }

        /// <summary>
        /// Attempts to change the user's display name. Follows PRG on success.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeDisplayName(string newDisplayName, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _profileService.TryChangeDisplayNameAsync(userId, newDisplayName, ct);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Display name updated!";
                return RedirectToAction(nameof(Index));
            }

            // Redisplay profile with error inside the modal (modal auto-opens)
            var profile = await _profileService.GetProfileAsync(userId, ct);
            ViewData["DisplayNameError"] = result.ErrorMessage;
            ViewData["OpenModal"] = true;
            ViewData["AttemptedDisplayName"] = newDisplayName;
            return View(nameof(Index), profile);
        }
    }
}
