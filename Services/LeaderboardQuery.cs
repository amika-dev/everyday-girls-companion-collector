using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Read-only query implementation for the leaderboard system.
    /// </summary>
    public class LeaderboardQuery : ILeaderboardQuery
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardQuery(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        /// <inheritdoc />
        public async Task<PagedResult<LeaderboardEntryDto>> GetTotalBondLeaderboardAsync(
            int page, int pageSize, CancellationToken ct)
        {
            var (clampedPage, clampedPageSize) = PagedResult<LeaderboardEntryDto>.Clamp(
                page, pageSize, GameConstants.LeaderboardPageSize);

            // Aggregate each user's total bond across all their companions, then join
            // with AspNetUsers to get display name and current partner info.
            // Ordered stably: highest total bond first, then display name ascending.
            var baseQuery = _context.UserGirls
                .GroupBy(ug => ug.UserId)
                .Select(g => new { UserId = g.Key, TotalBond = g.Sum(ug => ug.Bond) })
                .Join(
                    _context.Users,
                    x => x.UserId,
                    u => u.Id,
                    (x, u) => new
                    {
                        x.UserId,
                        u.DisplayName,
                        u.DisplayNameNormalized,
                        u.PartnerGirlId,
                        x.TotalBond
                    })
                .OrderByDescending(x => x.TotalBond)
                .ThenBy(x => x.DisplayNameNormalized);

            var totalCount = await baseQuery.CountAsync(ct);

            if (totalCount == 0)
            {
                return PagedResult<LeaderboardEntryDto>.Empty(clampedPage, clampedPageSize);
            }

            int skip = (clampedPage - 1) * clampedPageSize;

            // Fetch the current page. Partner name and image are resolved in the same
            // projection using correlated subqueries — no extra round-trips.
            var pageItems = await baseQuery
                .Skip(skip)
                .Take(clampedPageSize)
                .Select(x => new
                {
                    x.UserId,
                    x.DisplayName,
                    x.TotalBond,
                    PartnerName = _context.Girls
                        .Where(g => g.GirlId == x.PartnerGirlId)
                        .Select(g => (string?)g.Name)
                        .FirstOrDefault(),
                    PartnerImageUrl = _context.Girls
                        .Where(g => g.GirlId == x.PartnerGirlId)
                        .Select(g => (string?)g.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            // Compute globally consistent dense starting rank for this page.
            // Page 1 always starts at rank 1. For later pages, two server-side prefix
            // queries determine how many distinct score groups exist before this page
            // and whether the page boundary falls in the middle of a tie group.
            int startingRank;
            if (clampedPage == 1)
            {
                startingRank = 1;
            }
            else
            {
                // Score of the last item before this page (for tie detection at the boundary).
                var lastScoreBeforePage = await baseQuery
                    .Skip(skip - 1)
                    .Take(1)
                    .Select(x => (int?)x.TotalBond)
                    .FirstOrDefaultAsync(ct);

                // Number of distinct score values among all items before this page.
                int distinctScoreGroupsBeforePage = await baseQuery
                    .Take(skip)
                    .Select(x => x.TotalBond)
                    .Distinct()
                    .CountAsync(ct);

                int firstPageScore = pageItems.Count > 0 ? pageItems[0].TotalBond : 0;
                // If the boundary item shares a score with the first item on this page,
                // the page continues the same rank group; otherwise a new group starts.
                startingRank = lastScoreBeforePage.HasValue && lastScoreBeforePage.Value == firstPageScore
                    ? distinctScoreGroupsBeforePage
                    : distinctScoreGroupsBeforePage + 1;
            }

            var items = pageItems
                .Select(x => new LeaderboardEntryDto
                {
                    Rank = 0, // placeholder; dense rank assigned below
                    UserId = x.UserId,
                    DisplayName = x.DisplayName,
                    TotalBond = x.TotalBond,
                    PartnerName = x.PartnerName,
                    PartnerImageUrl = x.PartnerImageUrl
                })
                .ToList();

            items = AssignDenseRanks(items, startingRank, e => e.TotalBond);

            return new PagedResult<LeaderboardEntryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedPageSize
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<LeaderboardEntryDto>> GetCompanionLeaderboardAsync(
            int girlId, int page, int pageSize, CancellationToken ct)
        {
            var (clampedPage, clampedPageSize) = PagedResult<LeaderboardEntryDto>.Clamp(
                page, pageSize, GameConstants.LeaderboardPageSize);

            // Filter to only users who own this companion, then join with AspNetUsers to
            // get display name and current partner info.
            // Ordered stably: highest bond with this companion first, then display name ascending.
            var baseQuery = _context.UserGirls
                .Where(ug => ug.GirlId == girlId)
                .Join(
                    _context.Users,
                    ug => ug.UserId,
                    u => u.Id,
                    (ug, u) => new
                    {
                        ug.UserId,
                        u.DisplayName,
                        u.DisplayNameNormalized,
                        u.PartnerGirlId,
                        Bond = ug.Bond
                    })
                .OrderByDescending(x => x.Bond)
                .ThenBy(x => x.DisplayNameNormalized);

            var totalCount = await baseQuery.CountAsync(ct);

            if (totalCount == 0)
            {
                return PagedResult<LeaderboardEntryDto>.Empty(clampedPage, clampedPageSize);
            }

            int skip = (clampedPage - 1) * clampedPageSize;

            // Fetch the current page. Partner name and image are resolved in the same
            // projection using correlated subqueries — no extra round-trips.
            var pageItems = await baseQuery
                .Skip(skip)
                .Take(clampedPageSize)
                .Select(x => new
                {
                    x.UserId,
                    x.DisplayName,
                    x.Bond,
                    PartnerName = _context.Girls
                        .Where(g => g.GirlId == x.PartnerGirlId)
                        .Select(g => (string?)g.Name)
                        .FirstOrDefault(),
                    PartnerImageUrl = _context.Girls
                        .Where(g => g.GirlId == x.PartnerGirlId)
                        .Select(g => (string?)g.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            // Compute globally consistent dense starting rank for this page.
            // Page 1 always starts at rank 1. For later pages, two server-side prefix
            // queries determine how many distinct score groups exist before this page
            // and whether the page boundary falls in the middle of a tie group.
            int startingRank;
            if (clampedPage == 1)
            {
                startingRank = 1;
            }
            else
            {
                // Score of the last item before this page (for tie detection at the boundary).
                var lastScoreBeforePage = await baseQuery
                    .Skip(skip - 1)
                    .Take(1)
                    .Select(x => (int?)x.Bond)
                    .FirstOrDefaultAsync(ct);

                // Number of distinct score values among all items before this page.
                int distinctScoreGroupsBeforePage = await baseQuery
                    .Take(skip)
                    .Select(x => x.Bond)
                    .Distinct()
                    .CountAsync(ct);

                int firstPageScore = pageItems.Count > 0 ? pageItems[0].Bond : 0;
                // If the boundary item shares a score with the first item on this page,
                // the page continues the same rank group; otherwise a new group starts.
                startingRank = lastScoreBeforePage.HasValue && lastScoreBeforePage.Value == firstPageScore
                    ? distinctScoreGroupsBeforePage
                    : distinctScoreGroupsBeforePage + 1;
            }

            var items = pageItems
                .Select(x => new LeaderboardEntryDto
                {
                    Rank = 0, // placeholder; dense rank assigned below
                    UserId = x.UserId,
                    DisplayName = x.DisplayName,
                    TotalBond = x.Bond,
                    BondWithSelectedCompanion = x.Bond,
                    PartnerName = x.PartnerName,
                    PartnerImageUrl = x.PartnerImageUrl
                })
                .ToList();

            items = AssignDenseRanks(items, startingRank, e => e.BondWithSelectedCompanion ?? 0);

            return new PagedResult<LeaderboardEntryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedPageSize
            };
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CompanionOptionDto>> GetCompanionOptionsAsync(CancellationToken ct)
        {
            return await _context.Girls
                .OrderBy(g => g.Name)
                .Select(g => new CompanionOptionDto { GirlId = g.GirlId, Name = g.Name })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Applies dense ranking (1,2,2,3) to an already-sorted page of leaderboard entries.
        /// Equal scores share the same rank; the next distinct score increments rank by one.
        /// </summary>
        /// <param name="items">Pre-sorted entries for this page with placeholder ranks.</param>
        /// <param name="startingRank">The rank to assign to the first entry.</param>
        /// <param name="scoreSelector">Selects the score value used for rank comparison.</param>
        private static List<LeaderboardEntryDto> AssignDenseRanks(
            List<LeaderboardEntryDto> items, int startingRank, Func<LeaderboardEntryDto, int> scoreSelector)
        {
            if (items.Count == 0) return items;

            var result = new List<LeaderboardEntryDto>(items.Count);
            int currentRank = startingRank;
            int prevScore = scoreSelector(items[0]);

            for (int i = 0; i < items.Count; i++)
            {
                int score = scoreSelector(items[i]);
                if (i > 0 && score != prevScore)
                    currentRank++;
                result.Add(items[i] with { Rank = currentRank });
                prevScore = score;
            }

            return result;
        }
    }
}

