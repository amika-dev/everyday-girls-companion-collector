using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for FriendProfileQuery.GetFriendProfileAsync.
/// Verifies read-only profile data retrieval, partner panel fields, and account summary.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// </summary>
public class FriendProfileQueryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FriendProfileQuery _query;
    private readonly Mock<IClock> _clockMock;

    public FriendProfileQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _clockMock = new Mock<IClock>();
        // Default: 2026-01-20 19:00 UTC (after reset)
        _clockMock.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 20, 19, 0, 0, DateTimeKind.Utc));
        _query = new FriendProfileQuery(_context, _clockMock.Object);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<ApplicationUser> SeedUserAsync(
        string userId, string displayName, int? partnerGirlId = null)
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

    private async Task<Girl> SeedGirlAsync(int girlId, string name)
    {
        var girl = new Girl { GirlId = girlId, Name = name, ImageUrl = $"/images/{name.ToLowerInvariant()}.png" };
        _context.Girls.Add(girl);
        await _context.SaveChangesAsync();
        return girl;
    }

    private async Task SeedUserGirlAsync(string userId, int girlId, int bond = 0, PersonalityTag tag = PersonalityTag.Cheerful)
    {
        _context.UserGirls.Add(new UserGirl
        {
            UserId = userId,
            GirlId = girlId,
            Bond = bond,
            DateMetUtc = new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc),
            PersonalityTag = tag
        });
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // GetFriendProfileAsync
    // =========================================================================

    [Fact]
    public async Task GetFriendProfile_ReturnsNullWhenUserDoesNotExist()
    {
        await SeedUserAsync("viewer", "Viewer");

        var result = await _query.GetFriendProfileAsync("viewer", "nonexistent", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFriendProfile_ReturnsDisplayName()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        var result = await _query.GetFriendProfileAsync("viewer", "friend", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("FriendName", result.DisplayName);
        Assert.Equal("friend", result.FriendUserId);
    }

    [Fact]
    public async Task GetFriendProfile_PopulatesPartnerPanelFields()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName", partnerGirlId: 1);
        await SeedUserGirlAsync("friend", 1, bond: 25, tag: PersonalityTag.Shy);

        var result = await _query.GetFriendProfileAsync("viewer", "friend", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Sakura", result.PartnerName);
        Assert.Equal("/images/sakura.png", result.PartnerImagePath);
        Assert.Equal(25, result.PartnerBond);
        Assert.Equal(PersonalityTag.Shy, result.PartnerPersonalityTag);
        Assert.NotNull(result.PartnerFirstMetUtc);
        Assert.NotNull(result.PartnerDaysTogether);
    }

    [Fact]
    public async Task GetFriendProfile_NullPartnerFieldsWhenNoPartner()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        var result = await _query.GetFriendProfileAsync("viewer", "friend", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.PartnerName);
        Assert.Null(result.PartnerImagePath);
        Assert.Null(result.PartnerBond);
        Assert.Null(result.PartnerFirstMetUtc);
        Assert.Null(result.PartnerDaysTogether);
        Assert.Null(result.PartnerPersonalityTag);
    }

    [Fact]
    public async Task GetFriendProfile_PopulatesAccountSummary()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedGirlAsync(2, "Hana");
        await SeedGirlAsync(3, "Yuki");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");
        await SeedUserGirlAsync("friend", 1, bond: 10);
        await SeedUserGirlAsync("friend", 2, bond: 5);
        await SeedUserGirlAsync("friend", 3, bond: 3);

        var result = await _query.GetFriendProfileAsync("viewer", "friend", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.CompanionsCount);
        Assert.Equal(18, result.TotalBond);
    }

    [Fact]
    public async Task GetFriendProfile_ZeroStatsWhenNoCompanions()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        var result = await _query.GetFriendProfileAsync("viewer", "friend", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(0, result.CompanionsCount);
        Assert.Equal(0, result.TotalBond);
    }
}
