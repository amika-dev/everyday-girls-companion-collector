using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for FriendCollectionQuery (GetFriendCollectionAsync and GetFriendGirlDetailsAsync).
/// Verifies paged collection listing, detail DTO contents, and read-only constraints.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// </summary>
public class FriendCollectionQueryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FriendCollectionQuery _query;
    private readonly Mock<IClock> _clockMock;

    public FriendCollectionQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _clockMock = new Mock<IClock>();
        // Default: 2026-01-20 19:00 UTC (after reset)
        _clockMock.Setup(c => c.UtcNow).Returns(new DateTime(2026, 1, 20, 19, 0, 0, DateTimeKind.Utc));
        _query = new FriendCollectionQuery(_context, _clockMock.Object);
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

    private async Task SeedUserGirlAsync(
        string userId, int girlId, int bond = 0,
        PersonalityTag tag = PersonalityTag.Cheerful,
        DateTime? dateMetUtc = null,
        int charm = 0, int focus = 0, int vitality = 0)
    {
        _context.UserGirls.Add(new UserGirl
        {
            UserId = userId,
            GirlId = girlId,
            Bond = bond,
            DateMetUtc = dateMetUtc ?? new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc),
            PersonalityTag = tag,
            Charm = charm,
            Focus = focus,
            Vitality = vitality
        });
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // GetFriendCollectionAsync
    // =========================================================================

    [Fact]
    public async Task GetFriendCollection_ReturnsEmptyWhenNoCompanions()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        var result = await _query.GetFriendCollectionAsync("viewer", "friend", 1, 5, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetFriendCollection_ReturnsPagedResults()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        for (var i = 1; i <= 7; i++)
        {
            await SeedGirlAsync(i, $"Girl{i}");
            await SeedUserGirlAsync("friend", i, bond: 10 - i);
        }

        var page1 = await _query.GetFriendCollectionAsync("viewer", "friend", 1, 3, CancellationToken.None);
        var page2 = await _query.GetFriendCollectionAsync("viewer", "friend", 2, 3, CancellationToken.None);
        var page3 = await _query.GetFriendCollectionAsync("viewer", "friend", 3, 3, CancellationToken.None);

        Assert.Equal(7, page1.TotalCount);
        Assert.Equal(3, page1.Items.Count);
        Assert.Equal(3, page2.Items.Count);
        Assert.Equal(1, page3.Items.Count);
    }

    [Fact]
    public async Task GetFriendCollection_OrdersByBondDescThenDateMet()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        await SeedGirlAsync(1, "LowBond");
        await SeedGirlAsync(2, "HighBond");
        await SeedGirlAsync(3, "MidBond");

        await SeedUserGirlAsync("friend", 1, bond: 5);
        await SeedUserGirlAsync("friend", 2, bond: 20);
        await SeedUserGirlAsync("friend", 3, bond: 10);

        var result = await _query.GetFriendCollectionAsync("viewer", "friend", 1, 10, CancellationToken.None);

        Assert.Equal(3, result.Items.Count);
        Assert.Equal("HighBond", result.Items[0].Name);
        Assert.Equal("MidBond", result.Items[1].Name);
        Assert.Equal("LowBond", result.Items[2].Name);
    }

    [Fact]
    public async Task GetFriendCollection_MarksIsPartnerCorrectly()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedGirlAsync(2, "Hana");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName", partnerGirlId: 1);
        await SeedUserGirlAsync("friend", 1, bond: 10);
        await SeedUserGirlAsync("friend", 2, bond: 5);

        var result = await _query.GetFriendCollectionAsync("viewer", "friend", 1, 10, CancellationToken.None);

        var sakura = result.Items.Single(i => i.Name == "Sakura");
        var hana = result.Items.Single(i => i.Name == "Hana");

        Assert.True(sakura.IsPartner);
        Assert.False(hana.IsPartner);
    }

    [Fact]
    public async Task GetFriendCollection_PageLessThanOneClamps()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");
        await SeedUserGirlAsync("friend", 1, bond: 10);

        var result = await _query.GetFriendCollectionAsync("viewer", "friend", -3, 5, CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetFriendCollection_PageSizeZeroUsesDefault()
    {
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");
        for (var i = 1; i <= 10; i++)
        {
            await SeedGirlAsync(i, $"Girl{i}");
            await SeedUserGirlAsync("friend", i, bond: i);
        }

        var result = await _query.GetFriendCollectionAsync("viewer", "friend", 1, 0, CancellationToken.None);

        Assert.Equal(GameConstants.FriendsPageSize, result.PageSize);
        Assert.Equal(GameConstants.FriendsPageSize, result.Items.Count);
    }

    // =========================================================================
    // GetFriendGirlDetailsAsync
    // =========================================================================

    [Fact]
    public async Task GetFriendGirlDetails_ReturnsNullWhenGirlNotOwned()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");

        var result = await _query.GetFriendGirlDetailsAsync("viewer", "friend", 1, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFriendGirlDetails_ReturnsExpectedFields()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName", partnerGirlId: 1);
        await SeedUserGirlAsync("friend", 1, bond: 42, tag: PersonalityTag.Tsundere,
            charm: 5, focus: 3, vitality: 7);

        var result = await _query.GetFriendGirlDetailsAsync("viewer", "friend", 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.GirlId);
        Assert.Equal("Sakura", result.Name);
        Assert.Equal("/images/sakura.png", result.ImageUrl);
        Assert.Equal(42, result.Bond);
        Assert.Equal(PersonalityTag.Tsundere, result.PersonalityTag);
        Assert.True(result.IsPartner);
        Assert.Equal(5, result.Charm);
        Assert.Equal(3, result.Focus);
        Assert.Equal(7, result.Vitality);
    }

    [Fact]
    public async Task GetFriendGirlDetails_NotPartnerWhenDifferentPartner()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedGirlAsync(2, "Hana");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName", partnerGirlId: 2);
        await SeedUserGirlAsync("friend", 1, bond: 10);
        await SeedUserGirlAsync("friend", 2, bond: 20);

        var result = await _query.GetFriendGirlDetailsAsync("viewer", "friend", 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.IsPartner);
    }

    [Fact]
    public async Task GetFriendGirlDetails_IncludesDateMetAndDaysTogether()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("viewer", "Viewer");
        await SeedUserAsync("friend", "FriendName");
        await SeedUserGirlAsync("friend", 1, bond: 5,
            dateMetUtc: new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));

        var result = await _query.GetFriendGirlDetailsAsync("viewer", "friend", 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc), result.DateMetUtc);
        Assert.True(result.DaysTogether >= 0);
    }
}
