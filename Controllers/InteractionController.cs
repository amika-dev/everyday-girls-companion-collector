using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles daily partner interactions.
    /// </summary>
    [Authorize]
    public class InteractionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyStateService _dailyStateService;
        private readonly IDialogueService _dialogueService;

        public InteractionController(
            ApplicationDbContext context,
            IDailyStateService dailyStateService,
            IDialogueService dialogueService)
        {
            _context = context;
            _dailyStateService = dailyStateService;
            _dialogueService = dialogueService;
        }

        /// <summary>
        /// Display the interaction screen with partner information.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Fetch user with partner
            var user = await _context.Users
                .Include(u => u.Partner)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.PartnerGirlId == null || user.Partner == null)
            {
                TempData["Error"] = "There's no one to spend time with yet. Welcome your first companion home first ??";
                return RedirectToAction("Index", "Home");
            }

            // Fetch partner's bond and tag
            var partnerData = await _context.UserGirls
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GirlId == user.PartnerGirlId.Value);

            if (partnerData == null)
            {
                TempData["Error"] = "Hmm, something's not quite right. Try going back home first.";
                return RedirectToAction("Index", "Home");
            }

            // Fetch daily state
            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                dailyState = new Models.Entities.UserDailyState { UserId = userId };
                _context.UserDailyStates.Add(dailyState);
                await _context.SaveChangesAsync();
            }

            var viewModel = new InteractionViewModel
            {
                Partner = user.Partner,
                PartnerBond = partnerData.Bond,
                PartnerTag = partnerData.PersonalityTag,
                IsDailyInteractionAvailable = _dailyStateService.IsDailyInteractionAvailable(dailyState),
                TimeUntilReset = _dailyStateService.GetTimeUntilReset(),
                Dialogue = TempData["Dialogue"] as string
            };

            return View(viewModel);
        }

        /// <summary>
        /// Perform daily interaction: +1 bond, random dialogue.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Do()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Fetch user with partner
            var user = await _context.Users.FindAsync(userId);
            if (user?.PartnerGirlId == null)
            {
                TempData["Error"] = "There's no one to spend time with yet.";
                return RedirectToAction(nameof(Index));
            }

            // Fetch daily state
            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Check if Daily Interaction is available
            if (!_dailyStateService.IsDailyInteractionAvailable(dailyState))
            {
                TempData["Error"] = "You've already spent time together today. Come back tomorrow for more moments ??";
                return RedirectToAction(nameof(Index));
            }

            // Fetch partner data
            var partnerData = await _context.UserGirls
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GirlId == user.PartnerGirlId.Value);

            if (partnerData == null)
            {
                TempData["Error"] = "Hmm, something's not quite right. Try going back home first.";
                return RedirectToAction(nameof(Index));
            }

            // Increment bond
            partnerData.Bond += 1;

            // Mark Daily Interaction as used
            var serverDate = _dailyStateService.GetCurrentServerDate();
            dailyState.LastDailyInteractionDate = serverDate;

            await _context.SaveChangesAsync();

            // Get random dialogue
            var dialogue = _dialogueService.GetRandomDialogue(partnerData.PersonalityTag);
            TempData["Dialogue"] = dialogue;

            return RedirectToAction(nameof(Index));
        }
    }
}
