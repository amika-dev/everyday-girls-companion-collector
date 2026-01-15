using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyStateService _dailyStateService;

        public HomeController(ApplicationDbContext context, IDailyStateService dailyStateService)
        {
            _context = context;
            _dailyStateService = dailyStateService;
        }

        /// <summary>
        /// Main Menu (Hub) - displays daily status and partner information.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch or create UserDailyState
            var dailyState = await _context.UserDailyStates.FindAsync(userId);
            if (dailyState == null)
            {
                dailyState = new Models.Entities.UserDailyState { UserId = userId };
                _context.UserDailyStates.Add(dailyState);
                await _context.SaveChangesAsync();
            }

            // Fetch partner information
            var user = await _context.Users
                .Include(u => u.Partner)
                .FirstOrDefaultAsync(u => u.Id == userId);

            int? partnerBond = null;
            DateTime? partnerDateMet = null;
            Models.Enums.PersonalityTag? partnerTag = null;
            int? partnerDaysSinceAdoption = null;

            if (user?.PartnerGirlId != null)
            {
                var partnerData = await _context.UserGirls
                    .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GirlId == user.PartnerGirlId.Value);
                
                if (partnerData != null)
                {
                    partnerBond = partnerData.Bond;
                    partnerDateMet = partnerData.DateMetUtc;
                    partnerTag = partnerData.PersonalityTag;

                    var serverDate = _dailyStateService.GetCurrentServerDate();
                    partnerDaysSinceAdoption = DailyCadence.GetDaysSinceAdoption(serverDate, partnerData.DateMetUtc);
                }
            }

            var viewModel = new MainMenuViewModel
            {
                IsDailyRollAvailable = _dailyStateService.IsDailyRollAvailable(dailyState),
                IsDailyAdoptAvailable = _dailyStateService.IsDailyAdoptAvailable(dailyState),
                IsDailyInteractionAvailable = _dailyStateService.IsDailyInteractionAvailable(dailyState),
                TimeUntilReset = _dailyStateService.GetTimeUntilReset(),
                Partner = user?.Partner,
                PartnerBond = partnerBond,
                PartnerDateMet = partnerDateMet,
                PartnerTag = partnerTag,
                PartnerDaysSinceAdoption = partnerDaysSinceAdoption
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
