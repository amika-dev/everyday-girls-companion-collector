using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for LeaderboardQuery — dense ranking, tie-break ordering, and pagination.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// The dense-ranking helper (AssignDenseRanks) is tested indirectly via the public
/// GetTotalBondLeaderboardAsync / GetCompanionLeaderboardAsync methods.
/// </summary>
public class LeaderboardQueryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LeaderboardQuery _query;

    public LeaderboardQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _query = new LeaderboardQuery(_context);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<ApplicationUser> SeedUserAsync(string userId, string displayName, int? partnerGirlId = null)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            DisplayName = displayName,
            DisplayNameNormalized = displayName.ToUpperInvariant(),
            PartnerGirlId = partnerGirlId
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Girl> SeedGirlAsync(int girlId, string name, string? imageUrl = null)
    {
        var girl = new Girl { GirlId = girlId, Name = name, ImageUrl = imageUrl ?? string.Empty };
        _context.Girls.Add(girl);
        await _context.SaveChangesAsync();
        return girl;
    }

    private async Task SeedUserGirlAsync(string userId, int girlId, int bond)
    {
        _context.UserGirls.Add(new UserGirl
        {
            UserId = userId,
            GirlId = girlId,
            Bond = bond,
            DateMetUtc = DateTime.UtcNow,
            PersonalityTag = PersonalityTag.Cheerful
        });
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // TotalBond — Dense Ranking
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTotalBondLeaderboard_AssignsDenseRanks_WhenAllScoresAreDistinct()
    {
        // Arrange — 3 users with distinct total bonds
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Alice");
        await SeedUserAsync("u2", "Betty");
        await SeedUserAsync("u3", "Carol");
        await SeedUserGirlAsync("u1", 1, bond: 100);
        await SeedUserGirlAsync("u2", 1, bond: 50);
        await SeedUserGirlAsync("u3", 1, bond: 10);

        // Act
        var result = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — ranks 1, 2, 3 (no gaps, no ties)
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(2, result.Items[1].Rank);
        Assert.Equal(3, result.Items[2].Rank);
    }

    [Fact]
    public async Task GetTotalBondLeaderboard_AssignsSameRank_WhenTwoUsersHaveIdenticalScore()
    {
        // Arrange — users A and B tie; user C is below them
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Alice");
        await SeedUserAsync("u2", "Betty");
        await SeedUserAsync("u3", "Carol");
        await SeedUserGirlAsync("u1", 1, bond: 50);
        await SeedUserGirlAsync("u2", 1, bond: 50);
        await SeedUserGirlAsync("u3", 1, bond: 10);

        // Act
        var result = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — tied users share rank 1; next distinct score is rank 2 (dense, no gap)
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(1, result.Items[1].Rank);
        Assert.Equal(2, result.Items[2].Rank);
    }

    [Fact]
    public async Task GetTotalBondLeaderboard_PageBoundaryTie_BothSidesOfBoundaryShareSameRank()
    {
        // Arrange — 11 users where positions 10 and 11 share the same bond score.
        // With pageSize = 10, position 10 is the last entry on page 1 and position 11
        // is the first entry on page 2. Both must receive the same dense rank.
        await SeedGirlAsync(1, "Aoi");

        // Users 1-9: distinct decreasing bonds — ranks 1 through 9
        for (int i = 1; i <= 9; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: (10 - i) * 10); // 90, 80, ..., 10
        }

        // Users 10 and 11: tied — straddle the page boundary
        await SeedUserAsync("u10", "UserAA");
        await SeedUserAsync("u11", "UserBB");
        await SeedUserGirlAsync("u10", 1, bond: 5);
        await SeedUserGirlAsync("u11", 1, bond: 5);

        // Act — fetch both pages
        var page1 = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);
        var page2 = await _query.GetTotalBondLeaderboardAsync(2, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — last item on page 1 and first item on page 2 share the same rank
        var lastOnPage1 = page1.Items.Last();
        var firstOnPage2 = page2.Items.First();
        Assert.Equal(5, lastOnPage1.TotalBond);
        Assert.Equal(5, firstOnPage2.TotalBond);
        Assert.Equal(lastOnPage1.Rank, firstOnPage2.Rank);
    }

    [Fact]
    public async Task GetTotalBondLeaderboard_AfterPageBoundaryTie_NextDistinctScoreUsesNextDenseRank()
    {
        // Arrange — 12 users: 9 distinct, 2 tied at boundary (rank 10), 1 below the tie
        await SeedGirlAsync(1, "Aoi");

        for (int i = 1; i <= 9; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: (10 - i) * 10);
        }

        // Users 10 and 11 are tied at bond=5 — they straddle page 1/2 boundary
        await SeedUserAsync("u10", "UserAA");
        await SeedUserAsync("u11", "UserBB");
        await SeedUserGirlAsync("u10", 1, bond: 5);
        await SeedUserGirlAsync("u11", 1, bond: 5);

        // User 12 is below the tie — should get the next dense rank (11, not 12)
        await SeedUserAsync("u12", "UserCC");
        await SeedUserGirlAsync("u12", 1, bond: 1);

        // Act
        var page2 = await _query.GetTotalBondLeaderboardAsync(2, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Page 2 contains 2 items: the second tied user (bond=5, rank=10) and the user below
        // the tie (bond=1). The first tied user (UserAA) landed on page 1 at position 10.
        // Dense rank: both tied users share rank 10; the user below gets rank 11 (no gap).
        Assert.Equal(2, page2.Items.Count);
        Assert.Equal(10, page2.Items[0].Rank); // UserBB — second half of tie, same rank as UserAA on page 1
        Assert.Equal(11, page2.Items[1].Rank); // UserCC — next distinct score, rank increments by 1
    }

    // -------------------------------------------------------------------------
    // CompanionBond — Dense Ranking
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCompanionLeaderboard_AssignsDenseRanks_WhenAllScoresAreDistinct()
    {
        // Arrange
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Alice");
        await SeedUserAsync("u2", "Betty");
        await SeedUserAsync("u3", "Carol");
        await SeedUserGirlAsync("u1", 1, bond: 80);
        await SeedUserGirlAsync("u2", 1, bond: 40);
        await SeedUserGirlAsync("u3", 1, bond: 10);

        // Act
        var result = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(2, result.Items[1].Rank);
        Assert.Equal(3, result.Items[2].Rank);
    }

    [Fact]
    public async Task GetCompanionLeaderboard_AssignsSameRank_WhenTwoUsersHaveIdenticalBond()
    {
        // Arrange — A and B both have bond = 50 with the companion
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Alice");
        await SeedUserAsync("u2", "Betty");
        await SeedUserAsync("u3", "Carol");
        await SeedUserGirlAsync("u1", 1, bond: 50);
        await SeedUserGirlAsync("u2", 1, bond: 50);
        await SeedUserGirlAsync("u3", 1, bond: 10);

        // Act
        var result = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — tied users share rank 1; next distinct bond is rank 2
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Items[0].Rank);
        Assert.Equal(1, result.Items[1].Rank);
        Assert.Equal(2, result.Items[2].Rank);
    }

    [Fact]
    public async Task GetCompanionLeaderboard_PageBoundaryTie_BothSidesOfBoundaryShareSameRank()
    {
        // Arrange — 11 users with the companion; users 10 and 11 tied spanning the boundary
        await SeedGirlAsync(1, "Aoi");

        for (int i = 1; i <= 9; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: (10 - i) * 10);
        }

        await SeedUserAsync("u10", "UserAA");
        await SeedUserAsync("u11", "UserBB");
        await SeedUserGirlAsync("u10", 1, bond: 5);
        await SeedUserGirlAsync("u11", 1, bond: 5);

        // Act
        var page1 = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);
        var page2 = await _query.GetCompanionLeaderboardAsync(1, 2, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert
        var lastOnPage1 = page1.Items.Last();
        var firstOnPage2 = page2.Items.First();
        Assert.Equal(5, lastOnPage1.BondWithSelectedCompanion);
        Assert.Equal(5, firstOnPage2.BondWithSelectedCompanion);
        Assert.Equal(lastOnPage1.Rank, firstOnPage2.Rank);
    }

    // -------------------------------------------------------------------------
    // TotalBond — Stable Tie-Break Ordering
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTotalBondLeaderboard_WhenTied_OrdersByDisplayNameNormalizedAscending()
    {
        // Arrange — three users with identical total bond; expected order: Beta, Delta, Gamma
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Gamma"); // GAMMA
        await SeedUserAsync("u2", "Beta");  // BETA
        await SeedUserAsync("u3", "Delta"); // DELTA
        await SeedUserGirlAsync("u1", 1, bond: 50);
        await SeedUserGirlAsync("u2", 1, bond: 50);
        await SeedUserGirlAsync("u3", 1, bond: 50);

        // Act
        var result = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — alphabetical by DisplayNameNormalized; all share rank 1
        Assert.Equal("Beta", result.Items[0].DisplayName);
        Assert.Equal("Delta", result.Items[1].DisplayName);
        Assert.Equal("Gamma", result.Items[2].DisplayName);
        Assert.All(result.Items, e => Assert.Equal(1, e.Rank));
    }

    [Fact]
    public async Task GetCompanionLeaderboard_WhenTied_OrdersByDisplayNameNormalizedAscending()
    {
        // Arrange — three users with identical companion bond; expected order: Beta, Delta, Gamma
        await SeedGirlAsync(1, "Aoi");
        await SeedUserAsync("u1", "Gamma");
        await SeedUserAsync("u2", "Beta");
        await SeedUserAsync("u3", "Delta");
        await SeedUserGirlAsync("u1", 1, bond: 30);
        await SeedUserGirlAsync("u2", 1, bond: 30);
        await SeedUserGirlAsync("u3", 1, bond: 30);

        // Act
        var result = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert
        Assert.Equal("Beta", result.Items[0].DisplayName);
        Assert.Equal("Delta", result.Items[1].DisplayName);
        Assert.Equal("Gamma", result.Items[2].DisplayName);
        Assert.All(result.Items, e => Assert.Equal(1, e.Rank));
    }

    // -------------------------------------------------------------------------
    // Pagination — TotalBond
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetTotalBondLeaderboard_RespectsPageSize()
    {
        // Arrange — seed more users than page size
        await SeedGirlAsync(1, "Aoi");
        for (int i = 1; i <= 15; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: 100 - i);
        }

        // Act
        var result = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — items capped at page size
        Assert.Equal(GameConstants.LeaderboardPageSize, result.Items.Count);
    }

    [Fact]
    public async Task GetTotalBondLeaderboard_TotalCountIncludesAllUsers()
    {
        // Arrange — seed 15 users
        await SeedGirlAsync(1, "Aoi");
        for (int i = 1; i <= 15; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: 100 - i);
        }

        // Act
        var result = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
    }

    [Fact]
    public async Task GetTotalBondLeaderboard_Page2_ReturnsCorrectSlice()
    {
        // Arrange — 15 users with distinct bonds 99..85
        await SeedGirlAsync(1, "Aoi");
        for (int i = 1; i <= 15; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: 100 - i); // 99, 98, ..., 85
        }

        // Act
        var page1 = await _query.GetTotalBondLeaderboardAsync(1, GameConstants.LeaderboardPageSize, CancellationToken.None);
        var page2 = await _query.GetTotalBondLeaderboardAsync(2, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — page 2 items start where page 1 left off
        Assert.Equal(5, page2.Items.Count);
        Assert.True(
            page2.Items[0].TotalBond < page1.Items.Last().TotalBond,
            "Page 2 first item should have a lower bond than page 1 last item");
        Assert.Equal(11, page2.Items[0].Rank); // 11th distinct score = rank 11
    }

    // -------------------------------------------------------------------------
    // Pagination — CompanionBond
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetCompanionLeaderboard_RespectsPageSize()
    {
        // Arrange — 15 users with the companion
        await SeedGirlAsync(1, "Aoi");
        for (int i = 1; i <= 15; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: 100 - i);
        }

        // Act
        var result = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert
        Assert.Equal(GameConstants.LeaderboardPageSize, result.Items.Count);
    }

    [Fact]
    public async Task GetCompanionLeaderboard_TotalCountIncludesOnlyUsersWhoOwnTheCompanion()
    {
        // Arrange — 5 users own girl 1; 3 users own girl 2 only
        await SeedGirlAsync(1, "Aoi");
        await SeedGirlAsync(2, "Hana");

        for (int i = 1; i <= 5; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 1, bond: 50 - i);
        }

        for (int i = 6; i <= 8; i++)
        {
            await SeedUserAsync($"u{i}", $"User{i:D2}");
            await SeedUserGirlAsync($"u{i}", 2, bond: 10);
        }

        // Act — leaderboard scoped to girl 1
        var result = await _query.GetCompanionLeaderboardAsync(1, 1, GameConstants.LeaderboardPageSize, CancellationToken.None);

        // Assert — only 5 users appear (those who own girl 1)
        Assert.Equal(5, result.TotalCount);
    }
}
