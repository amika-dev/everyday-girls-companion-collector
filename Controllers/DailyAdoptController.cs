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

            var viewModel = new DailyAdoptViewModel
            {
                IsDailyRollAvailable = _dailyStateService.IsDailyRollAvailable(dailyState),
                IsDailyAdoptAvailable = _dailyStateService.IsDailyAdoptAvailable(dailyState),
                Candidates = candidates,
                TimeUntilReset = _dailyStateService.GetTimeUntilReset(),
                OwnedGirlsCount = ownedCount
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
                .ToListAsync();

            Random.Shared.Shuffle(allCandidates);
            var candidates = allCandidates
                .Take(5)
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
                TempData["Error"] = "Daily Roll must be used before adopting.";
                return RedirectToAction(nameof(Index));
            }

            if (dailyState.CandidateDate != serverDate)
            {
                TempData["Error"] = "Today's candidates are not available.";
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
                TempData["Error"] = "Selected girl is not a valid candidate.";
                return RedirectToAction(nameof(Index));
            }

            if (!_dailyStateService.IsDailyAdoptAvailable(dailyState))
            {
                TempData["Error"] = "Daily Adopt has already been used today.";
                return RedirectToAction(nameof(Index));
            }

            var ownedCount = await _context.UserGirls.CountAsync(ug => ug.UserId == userId);
            if (ownedCount >= 100)
            {
                TempData["Error"] = "Collection limit reached (100). Abandon a girl to adopt new ones.";
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

            await _context.SaveChangesAsync();

            TempData["Success"] = "Girl adopted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
