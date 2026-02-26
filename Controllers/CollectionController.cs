using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles collection viewing, sorting, and girl management.
    /// </summary>
    [Authorize]
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyStateService _dailyStateService;
        private const int PageSize = GameConstants.CollectionPageSize;

        public CollectionController(ApplicationDbContext context, IDailyStateService dailyStateService)
        {
            _context = context;
            _dailyStateService = dailyStateService;
        }

        /// <summary>
        /// Display collection grid with sorting and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string sort = "bond", int page = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Fetch user's partner ID
            var user = await _context.Users.FindAsync(userId);
            var partnerGirlId = user?.PartnerGirlId;

            // Query owned girls with sorting
            var query = _context.UserGirls
                .Include(ug => ug.Girl)
                .Where(ug => ug.UserId == userId);

            query = sort.ToLower() switch
            {
                "oldest" => query.OrderBy(ug => ug.DateMetUtc),
                "newest" => query.OrderByDescending(ug => ug.DateMetUtc),
                _ => query.OrderByDescending(ug => ug.Bond).ThenBy(ug => ug.DateMetUtc) // bond (default)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            // Ensure page is within bounds
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var serverDate = _dailyStateService.GetCurrentServerDate();

            var girls = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(ug => new CollectionGirlViewModel
                {
                    GirlId = ug.GirlId,
                    Name = ug.Girl.Name,
                    ImageUrl = ug.Girl.ImageUrl,
                    Bond = ug.Bond,
                    DateMetUtc = ug.DateMetUtc,
                    PersonalityTag = ug.PersonalityTag,
                    IsPartner = ug.GirlId == partnerGirlId,
                    DaysSinceAdoption = DailyCadence.GetDaysSinceAdoption(serverDate, ug.DateMetUtc)
                })
                .ToListAsync();

            var viewModel = new CollectionViewModel
            {
                Girls = girls,
                SortMode = sort,
                CurrentPage = page,
                TotalPages = totalPages,
                PartnerGirlId = partnerGirlId,
                PersonalityTagMap = Enum.GetValues<PersonalityTag>()
                    .ToDictionary(t => t.ToString(), t => (int)t)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Set a girl as the current partner.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPartner(int girlId, string? returnSort, int? returnPage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Verify user owns the girl
            var ownsGirl = await _context.UserGirls
                .AnyAsync(ug => ug.UserId == userId && ug.GirlId == girlId);

            if (!ownsGirl)
            {
                TempData["Error"] = "Hmm, it looks like she's not part of your collection yet.";
                return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
            }

            // Set as partner
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PartnerGirlId = girlId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "She's your partner now ✨ Spend time together whenever you like.";
            }

            return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
        }

        /// <summary>
        /// Change a girl's personality tag.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTag(int girlId, PersonalityTag tag, string? returnSort, int? returnPage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var userGirl = await _context.UserGirls
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GirlId == girlId);

            if (userGirl == null)
            {
                TempData["Error"] = "Hmm, it looks like she's not part of your collection yet.";
                return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
            }

            userGirl.PersonalityTag = tag;
            await _context.SaveChangesAsync();
            TempData["Success"] = "That feels right!";

            return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
        }

        /// <summary>
        /// Abandon a girl (delete from collection). Partner cannot be abandoned.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abandon(int girlId, string? returnSort, int? returnPage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Verify user owns the girl
            var userGirl = await _context.UserGirls
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GirlId == girlId);

            if (userGirl == null)
            {
                TempData["Error"] = "Hmm, it looks like she's not part of your collection yet.";
                return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
            }

            // Check if girl is partner
            var user = await _context.Users.FindAsync(userId);
            if (user?.PartnerGirlId == girlId)
            {
                TempData["Error"] = "You can't part ways with your partner. Choose someone else first.";
                return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
            }

            // Delete the girl
            _context.UserGirls.Remove(userGirl);
            await _context.SaveChangesAsync();
            TempData["Success"] = "You've parted ways. She'll be okay.";

            return RedirectToAction(nameof(Index), new { sort = returnSort ?? "bond", page = returnPage ?? 1 });
        }
    }
}
