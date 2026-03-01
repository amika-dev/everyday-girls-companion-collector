using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Read-only recognition layer for player and companion leaderboards.
    /// All endpoints are GET-only; no mutations are performed.
    /// </summary>
    [Authorize]
    public class LeaderboardsController : Controller
    {
        private readonly ILeaderboardQuery _leaderboardQuery;

        public LeaderboardsController(ILeaderboardQuery leaderboardQuery)
        {
            ArgumentNullException.ThrowIfNull(leaderboardQuery);
            _leaderboardQuery = leaderboardQuery;
        }

        /// <summary>
        /// Unified leaderboard entry point. Routes to the appropriate board based on type.
        /// GET /Leaderboards?type=TotalBond
        /// GET /Leaderboards?type=CompanionBond&amp;girlId=1&amp;page=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            LeaderboardType type = LeaderboardType.TotalBond,
            int? girlId = null,
            int page = 1,
            CancellationToken ct = default)
        {
            if (type == LeaderboardType.TotalBond)
            {
                var results = await _leaderboardQuery.GetTotalBondLeaderboardAsync(
                    page, GameConstants.LeaderboardPageSize, ct);

                return View(new LeaderboardViewModel
                {
                    Title = "Total Bond",
                    Subtitle = "A record of the bonds that have grown over time",
                    Type = LeaderboardType.TotalBond,
                    Filter = new LeaderboardFilterDto { Type = type, Page = page },
                    Results = results,
                    Companions = []
                });
            }

            // CompanionBond — load options first, then default to first available.
            var companions = await _leaderboardQuery.GetCompanionOptionsAsync(ct);
            var selectedGirlId = girlId ?? companions.FirstOrDefault()?.GirlId;

            if (selectedGirlId is null)
            {
                return View(new LeaderboardViewModel
                {
                    Title = "Companion Bond",
                    Subtitle = "Bond rankings for a selected companion",
                    Type = LeaderboardType.CompanionBond,
                    Filter = new LeaderboardFilterDto { Type = type, GirlId = null, Page = page },
                    Results = Models.PagedResult<LeaderboardEntryDto>.Empty(page, GameConstants.LeaderboardPageSize),
                    Companions = companions
                });
            }

            var companionResults = await _leaderboardQuery.GetCompanionLeaderboardAsync(
                selectedGirlId.Value, page, GameConstants.LeaderboardPageSize, ct);

            var selectedCompanion = companions.FirstOrDefault(c => c.GirlId == selectedGirlId);

            return View(new LeaderboardViewModel
            {
                Title = selectedCompanion is not null
                    ? $"Bond with {selectedCompanion.Name}"
                    : "Companion Bond",
                Subtitle = selectedCompanion is not null
                    ? $"Those who have grown especially close to {selectedCompanion.Name}"
                    : "Bond rankings for a selected companion",
                Type = LeaderboardType.CompanionBond,
                Filter = new LeaderboardFilterDto { Type = type, GirlId = selectedGirlId, Page = page },
                Results = companionResults,
                Companions = companions
            });
        }

        /// <summary>Redirects legacy /Leaderboards/Players route to the unified index.</summary>
        [HttpGet]
        public IActionResult Players(int page = 1) =>
            RedirectToAction(nameof(Index), new { type = LeaderboardType.TotalBond, page });

        /// <summary>Redirects legacy /Leaderboards/Companions route to the unified index.</summary>
        [HttpGet]
        public IActionResult Companions(int? girlId = null, int page = 1) =>
            RedirectToAction(nameof(Index), new { type = LeaderboardType.CompanionBond, girlId, page });
    }
}
