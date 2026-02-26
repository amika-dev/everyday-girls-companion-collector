using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for ProfileService.
/// Verifies profile summary queries and display name change business rules.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// Time: Mock<IClock> for deterministic ServerDate control.
///
/// ServerDate rule: at 12:00 UTC on 2026-01-15, ServerDate = 2026-01-14 (before 18:00 reset).
/// </summary>
public class ProfileServiceTests : IDisposable
{
    // At 12:00 UTC, ServerDate = 2026-01-14 (before 18:00 reset).
    private static readonly DateTime TestTimeUtc = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly TestServerDate = new(2026, 1, 14);

    private readonly ApplicationDbContext _context;
    private readonly Mock<IClock> _mockClock;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockClock = new Mock<IClock>();
        _mockClock.Setup(c => c.UtcNow).Returns(TestTimeUtc);

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
            null, null, null, null);

        _service = new ProfileService(_context, _mockClock.Object, signInManagerMock.Object);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<ApplicationUser> SeedUserAsync(
        string userId = "user1",
        string displayName = "TestUser",
        DateTime? lastDisplayNameChangeUtc = null,
        int? partnerGirlId = null)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            DisplayName = displayName,
            DisplayNameNormalized = displayName.ToUpperInvariant(),
            LastDisplayNameChangeUtc = lastDisplayNameChangeUtc,
            PartnerGirlId = partnerGirlId
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Girl> SeedGirlAsync(int girlId, string name = "Akira")
    {
        var girl = new Girl { GirlId = girlId, Name = name, ImageUrl = $"/images/{name.ToLower()}.png" };
        _context.Girls.Add(girl);
        await _context.SaveChangesAsync();
        return girl;
    }

    private async Task SeedUserGirlAsync(
        string userId, int girlId, int bond = 0,
        DateTime? dateMetUtc = null, PersonalityTag tag = PersonalityTag.Cheerful)
    {
        _context.UserGirls.Add(new UserGirl
        {
            UserId = userId,
            GirlId = girlId,
            Bond = bond,
            DateMetUtc = dateMetUtc ?? TestTimeUtc,
            PersonalityTag = tag
        });
        await _context.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // GetProfileAsync — collection totals
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_WithNoCompanions_ReturnsTotalsOfZero()
    {
        await SeedUserAsync();

        var result = await _service.GetProfileAsync("user1");

        Assert.Equal(0, result.TotalBond);
        Assert.Equal(0, result.TotalCompanions);
    }

    [Fact]
    public async Task GetProfileAsync_WithMultipleCompanions_ReturnsSummedBondAndCount()
    {
        await SeedGirlAsync(1);
        await SeedGirlAsync(2);
        await SeedGirlAsync(3);
        await SeedUserAsync();
        await SeedUserGirlAsync("user1", 1, bond: 10);
        await SeedUserGirlAsync("user1", 2, bond: 25);
        await SeedUserGirlAsync("user1", 3, bond: 5);

        var result = await _service.GetProfileAsync("user1");

        Assert.Equal(40, result.TotalBond);
        Assert.Equal(3, result.TotalCompanions);
    }

    // -------------------------------------------------------------------------
    // GetProfileAsync — partner fields
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_WithNoPartner_ReturnsNullPartnerFields()
    {
        await SeedUserAsync(partnerGirlId: null);

        var result = await _service.GetProfileAsync("user1");

        Assert.Null(result.PartnerName);
        Assert.Null(result.PartnerImageUrl);
        Assert.Null(result.PartnerBond);
        Assert.Null(result.PartnerFirstMetUtc);
        Assert.Null(result.PartnerDaysTogether);
        Assert.Null(result.PartnerPersonalityTag);
    }

    [Fact]
    public async Task GetProfileAsync_WithPartner_ReturnsPartnerDetails()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync(partnerGirlId: 1);
        await SeedUserGirlAsync("user1", 1, bond: 42, tag: PersonalityTag.Shy);

        var result = await _service.GetProfileAsync("user1");

        Assert.Equal("Sakura", result.PartnerName);
        Assert.Equal("/images/sakura.png", result.PartnerImageUrl);
        Assert.Equal(42, result.PartnerBond);
        Assert.Equal(TestTimeUtc, result.PartnerFirstMetUtc);
        Assert.Equal(PersonalityTag.Shy, result.PartnerPersonalityTag);
    }

    [Fact]
    public async Task GetProfileAsync_WithPartner_ReturnsDaysTogether()
    {
        // Partner adopted 3 ServerDates ago: Jan 11 at 19:00 UTC → ServerDate = Jan 11
        // Current ServerDate = Jan 14 → DaysTogether = 3
        var adoptedUtc = new DateTime(2026, 1, 11, 19, 0, 0, DateTimeKind.Utc);
        await SeedGirlAsync(1, "Hana");
        await SeedUserAsync(partnerGirlId: 1);
        await SeedUserGirlAsync("user1", 1, bond: 5, dateMetUtc: adoptedUtc);

        var result = await _service.GetProfileAsync("user1");

        Assert.Equal(3, result.PartnerDaysTogether);
    }

    [Fact]
    public async Task GetProfileAsync_PartnerBondIsNotIncludedInTotalBondTwice()
    {
        await SeedGirlAsync(1, "Yuki");
        await SeedGirlAsync(2, "Hana");
        await SeedUserAsync(partnerGirlId: 1);
        await SeedUserGirlAsync("user1", 1, bond: 10);
        await SeedUserGirlAsync("user1", 2, bond: 20);

        var result = await _service.GetProfileAsync("user1");

        // Total bond is sum of all, not double-counted
        Assert.Equal(30, result.TotalBond);
        Assert.Equal(10, result.PartnerBond);
    }

    // -------------------------------------------------------------------------
    // GetProfileAsync — CanChangeDisplayName
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_WhenNeverChangedDisplayName_CanChangeDisplayNameIsTrue()
    {
        await SeedUserAsync(lastDisplayNameChangeUtc: null);

        var result = await _service.GetProfileAsync("user1");

        Assert.True(result.CanChangeDisplayName);
    }

    [Fact]
    public async Task GetProfileAsync_WhenChangedDisplayNameThisServerDate_CanChangeDisplayNameIsFalse()
    {
        // Last changed at 10:00 UTC on Jan 15 → ServerDate = Jan 14 (same as test ServerDate)
        var lastChanged = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        await SeedUserAsync(lastDisplayNameChangeUtc: lastChanged);

        var result = await _service.GetProfileAsync("user1");

        Assert.False(result.CanChangeDisplayName);
    }

    [Fact]
    public async Task GetProfileAsync_WhenChangedDisplayNamePreviousServerDate_CanChangeDisplayNameIsTrue()
    {
        // Last changed at 10:00 UTC on Jan 14 → ServerDate = Jan 13 (before test ServerDate of Jan 14)
        var lastChanged = new DateTime(2026, 1, 14, 10, 0, 0, DateTimeKind.Utc);
        await SeedUserAsync(lastDisplayNameChangeUtc: lastChanged);

        var result = await _service.GetProfileAsync("user1");

        Assert.True(result.CanChangeDisplayName);
    }

    // -------------------------------------------------------------------------
    // TryChangeDisplayNameAsync — format validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("abc")]         // 3 chars — too short
    [InlineData("")]            // empty
    [InlineData("   ")]         // whitespace
    [InlineData("has space")]   // contains space
    [InlineData("under_score")] // contains underscore
    [InlineData("has-hyphen")]  // contains hyphen
    [InlineData("toolongname12345x")] // 17 chars — too long
    public async Task TryChangeDisplayNameAsync_InvalidFormat_Fails(string invalidName)
    {
        await SeedUserAsync();

        var result = await _service.TryChangeDisplayNameAsync("user1", invalidName);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("Abcd")]           // exactly 4 chars
    [InlineData("ValidName123")]   // mixed alphanumeric
    [InlineData("ABCDEFGH12345678")] // exactly 16 chars — at the max length boundary
    [InlineData("ShortName1")]     // 9 chars
    public async Task TryChangeDisplayNameAsync_ValidFormat_DoesNotFailOnFormat(string validName)
    {
        // Use a name that won't collide or hit other rules
        await SeedUserAsync(displayName: "OldName1");

        var result = await _service.TryChangeDisplayNameAsync("user1", validName);

        // May fail for uniqueness but not for format — verify it's not a format error
        if (!result.Succeeded)
        {
            Assert.DoesNotContain("alphanumeric", result.ErrorMessage);
        }
    }

    // -------------------------------------------------------------------------
    // TryChangeDisplayNameAsync — same name
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryChangeDisplayNameAsync_SameNameExactCase_Fails()
    {
        await SeedUserAsync(displayName: "TestUser1");

        var result = await _service.TryChangeDisplayNameAsync("user1", "TestUser1");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task TryChangeDisplayNameAsync_SameNameDifferentCase_Fails()
    {
        await SeedUserAsync(displayName: "TestUser1");

        var result = await _service.TryChangeDisplayNameAsync("user1", "testuser1");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // TryChangeDisplayNameAsync — once per reset
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryChangeDisplayNameAsync_AlreadyChangedThisServerDate_Fails()
    {
        // Last changed at 10:00 UTC on Jan 15 → ServerDate = Jan 14 = current ServerDate
        var lastChanged = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        await SeedUserAsync(displayName: "OldName1", lastDisplayNameChangeUtc: lastChanged);

        var result = await _service.TryChangeDisplayNameAsync("user1", "NewName1");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task TryChangeDisplayNameAsync_ChangedOnPreviousServerDate_Succeeds()
    {
        // Last changed at 10:00 UTC on Jan 14 → ServerDate = Jan 13 ≠ current Jan 14
        var lastChanged = new DateTime(2026, 1, 14, 10, 0, 0, DateTimeKind.Utc);
        await SeedUserAsync(displayName: "OldName1", lastDisplayNameChangeUtc: lastChanged);

        var result = await _service.TryChangeDisplayNameAsync("user1", "NewName1");

        Assert.True(result.Succeeded);
    }

    // -------------------------------------------------------------------------
    // TryChangeDisplayNameAsync — uniqueness
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryChangeDisplayNameAsync_NameTakenByAnotherUser_Fails()
    {
        await SeedUserAsync(userId: "user1", displayName: "PlayerOne");
        await SeedUserAsync(userId: "user2", displayName: "PlayerTwo");

        // user1 tries to take user2's name
        var result = await _service.TryChangeDisplayNameAsync("user1", "PlayerTwo");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task TryChangeDisplayNameAsync_NameTakenByAnotherUserDifferentCase_Fails()
    {
        await SeedUserAsync(userId: "user1", displayName: "PlayerOne");
        await SeedUserAsync(userId: "user2", displayName: "PlayerTwo");

        var result = await _service.TryChangeDisplayNameAsync("user1", "playertwo");

        Assert.False(result.Succeeded);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // TryChangeDisplayNameAsync — success and field persistence
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryChangeDisplayNameAsync_ValidChange_Succeeds()
    {
        await SeedUserAsync(displayName: "OldName1");

        var result = await _service.TryChangeDisplayNameAsync("user1", "NewName1");

        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task TryChangeDisplayNameAsync_WhenSucceeds_PersistsAllThreeFields()
    {
        await SeedUserAsync(displayName: "OldName1");

        await _service.TryChangeDisplayNameAsync("user1", "NewName1");

        var updated = await _context.Users.FindAsync("user1");
        Assert.Equal("NewName1", updated!.DisplayName);
        Assert.Equal("NEWNAME1", updated.DisplayNameNormalized);
        Assert.Equal(TestTimeUtc, updated.LastDisplayNameChangeUtc);
    }

    [Fact]
    public async Task TryChangeDisplayNameAsync_NeverChanged_Succeeds()
    {
        await SeedUserAsync(displayName: "OldName1", lastDisplayNameChangeUtc: null);

        var result = await _service.TryChangeDisplayNameAsync("user1", "NewName1");

        Assert.True(result.Succeeded);
    }
}
