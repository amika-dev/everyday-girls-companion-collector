using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles Daily Roll and Daily Adopt functionality.
    /// </summary>
    [Authorize]
    public class DailyAdoptController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyStateService _dailyStateService;

        public DailyAdoptController(ApplicationDbContext context, IDailyStateService dailyStateService)
        {
            _context = context;
            _dailyStateService = dailyStateService;
        }

        /// <summary>
        /// Display the Daily Roll/Adopt screen.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                dailyState = new UserDailyState { UserId = userId };
                _context.UserDailyStates.Add(dailyState);
                await _context.SaveChangesAsync();
            }

            var serverDate = _dailyStateService.GetCurrentServerDate();
            var candidates = new List<Girl>();

            // Load candidates only if Daily Roll was used AND CandidateDate matches ServerDate
            if (!_dailyStateService.IsDailyRollAvailable(dailyState) && dailyState.CandidateDate == serverDate)
            {
                var candidateIds = new List<int?>
                {
                    dailyState.Candidate1GirlId,
                    dailyState.Candidate2GirlId,
                    dailyState.Candidate3GirlId,
                    dailyState.Candidate4GirlId,
                    dailyState.Candidate5GirlId
                }.Where(id => id.HasValue).Select(id => id!.Value).ToList();

                if (candidateIds.Any())
                {
                    candidates = await _context.Girls
                        .Where(g => candidateIds.Contains(g.GirlId))
                        .ToListAsync();
                }
            }

            var ownedCount = await _context.UserGirls.CountAsync(ug => ug.UserId == userId);

            // Load today's adopted girl if available
            Girl? todayAdoptedGirl = null;
            if (!_dailyStateService.IsDailyAdoptAvailable(dailyState) && dailyState.TodayAdoptedGirlId.HasValue)
            {
                todayAdoptedGirl = await _context.Girls.FindAsync(dailyState.TodayAdoptedGirlId.Value);
            }

            var viewModel = new DailyAdoptViewModel
            {
                IsDailyRollAvailable = _dailyStateService.IsDailyRollAvailable(dailyState),
                IsDailyAdoptAvailable = _dailyStateService.IsDailyAdoptAvailable(dailyState),
                Candidates = candidates,
                TimeUntilReset = _dailyStateService.GetTimeUntilReset(),
                OwnedGirlsCount = ownedCount,
                TodayAdoptedGirl = todayAdoptedGirl
            };

            return View(viewModel);
        }

        /// <summary>
        /// Use Daily Roll to generate 5 random candidates.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRoll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Check if Daily Roll is available
            if (!_dailyStateService.IsDailyRollAvailable(dailyState))
            {
                return RedirectToAction(nameof(Index));
            }

            var serverDate = _dailyStateService.GetCurrentServerDate();

            // Get owned girl IDs
            var ownedGirlIds = await _context.UserGirls
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.GirlId)
                .ToListAsync();

            // Select 5 random girls not owned by user
            var allCandidates = await _context.Girls
                .Where(g => !ownedGirlIds.Contains(g.GirlId))
                .ToArrayAsync();

            Random.Shared.Shuffle(allCandidates);
            var candidates = allCandidates
                .Take(GameConstants.DailyCandidateCount)
                .ToList();
            // Persist candidates
            dailyState.LastDailyRollDate = serverDate;
            dailyState.CandidateDate = serverDate;
            dailyState.Candidate1GirlId = candidates.ElementAtOrDefault(0)?.GirlId;
            dailyState.Candidate2GirlId = candidates.ElementAtOrDefault(1)?.GirlId;
            dailyState.Candidate3GirlId = candidates.ElementAtOrDefault(2)?.GirlId;
            dailyState.Candidate4GirlId = candidates.ElementAtOrDefault(3)?.GirlId;
            dailyState.Candidate5GirlId = candidates.ElementAtOrDefault(4)?.GirlId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Adopt one of the daily candidates.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adopt(int girlId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var serverDate = _dailyStateService.GetCurrentServerDate();

            // Precondition checks
            if (_dailyStateService.IsDailyRollAvailable(dailyState))
            {
                TempData["Error"] = "Let's see who's here today first";
                return RedirectToAction(nameof(Index));
            }

            if (dailyState.CandidateDate != serverDate)
            {
                TempData["Error"] = "Those companions aren't around right now. Check back later ✨";
                return RedirectToAction(nameof(Index));
            }

            var candidateIds = new List<int?>
            {
                dailyState.Candidate1GirlId,
                dailyState.Candidate2GirlId,
                dailyState.Candidate3GirlId,
                dailyState.Candidate4GirlId,
                dailyState.Candidate5GirlId
            };

            if (!candidateIds.Contains(girlId))
            {
                TempData["Error"] = "Hmm, that's not one of today's visitors.";
                return RedirectToAction(nameof(Index));
            }

            if (!_dailyStateService.IsDailyAdoptAvailable(dailyState))
            {
                TempData["Error"] = "You've already welcomed someone home today. Come back tomorrow for more 💖";
                return RedirectToAction(nameof(Index));
            }

            var ownedCount = await _context.UserGirls.CountAsync(ug => ug.UserId == userId);
            if (ownedCount >= GameConstants.MaxCollectionSize)
            {
                TempData["Error"] = "Your home is full right now. To welcome someone new, you'll need to part ways with someone else first (not your partner though!)";
                return RedirectToAction(nameof(Index));
            }

            // Adopt the girl
            var userGirl = new UserGirl
            {
                UserId = userId,
                GirlId = girlId,
                DateMetUtc = DateTime.UtcNow,
                Bond = 0,
                PersonalityTag = PersonalityTag.Cheerful // Default to first enum value
            };

            _context.UserGirls.Add(userGirl);

            // If this is first adoption, set as partner
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.PartnerGirlId == null)
            {
                user.PartnerGirlId = girlId;
            }

            // Mark Daily Adopt as used
            dailyState.LastDailyAdoptDate = serverDate;
            dailyState.TodayAdoptedGirlId = girlId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "She's happy to be with you now ✨";
            return RedirectToAction(nameof(Index));
        }
    }
}
