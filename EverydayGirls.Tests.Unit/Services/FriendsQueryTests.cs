using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for FriendsQuery (GetFriendsAsync and SearchUsersByDisplayNameAsync).
/// Verifies friend list ordering, search behavior, friendship marking, and take limits.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// </summary>
public class FriendsQueryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FriendsQuery _query;

    public FriendsQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _query = new FriendsQuery(_context);
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

    private async Task SeedUserGirlAsync(string userId, int girlId, int bond = 0)
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

    private async Task SeedFriendRelationshipAsync(string userId, string friendUserId)
    {
        _context.FriendRelationships.Add(new FriendRelationship
        {
            UserId = userId,
            FriendUserId = friendUserId,
            DateAddedUtc = DateTime.UtcNow
        });
        _context.FriendRelationships.Add(new FriendRelationship
        {
            UserId = friendUserId,
            FriendUserId = userId,
            DateAddedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // SearchUsersByDisplayNameAsync
    // =========================================================================

    // -------------------------------------------------------------------------
    // StartsWith is case-insensitive via normalized field
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_StartsWithIsCaseInsensitive()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "ALICIA");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "ali", 25, CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.StartsWith("ALI", r.DisplayName.ToUpperInvariant()));
    }

    // -------------------------------------------------------------------------
    // Trims input
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_TrimsWhitespaceFromInput()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "  Alice  ", 25, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Alice", results[0].DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchUsers_EmptyOrWhitespaceInput_ReturnsEmptyList(string? searchText)
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", searchText!, 25, CancellationToken.None);

        Assert.Empty(results);
    }

    // -------------------------------------------------------------------------
    // Excludes self
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_ExcludesSelf()
    {
        await SeedUserAsync("requester", "Alice");
        await SeedUserAsync("user1", "Alicia");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "Ali", 25, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("user1", results[0].UserId);
    }

    // -------------------------------------------------------------------------
    // Marks IsAlreadyFriend and CanAdd correctly
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_MarksIsAlreadyFriendCorrectly()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("friend1", "Alpha");
        await SeedUserAsync("stranger1", "Amber");
        await SeedFriendRelationshipAsync("requester", "friend1");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "A", 25, CancellationToken.None);

        var friendResult = results.Single(r => r.UserId == "friend1");
        var strangerResult = results.Single(r => r.UserId == "stranger1");

        Assert.True(friendResult.IsAlreadyFriend);
        Assert.False(friendResult.CanAdd);
        Assert.False(strangerResult.IsAlreadyFriend);
        Assert.True(strangerResult.CanAdd);
    }

    // -------------------------------------------------------------------------
    // Respects take limit
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_RespectsMaxTakeLimit()
    {
        await SeedUserAsync("requester", "Requester");
        for (var i = 0; i < 30; i++)
        {
            await SeedUserAsync($"user{i}", $"Alpha{i:D2}");
        }

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 50, CancellationToken.None);

        // Should be clamped to 25
        Assert.Equal(25, results.Count);
    }

    [Fact]
    public async Task SearchUsers_TakeOfTwo_ReturnsTwoResults()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alpha1");
        await SeedUserAsync("user2", "Alpha2");
        await SeedUserAsync("user3", "Alpha3");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 2, CancellationToken.None);

        Assert.Equal(2, results.Count);
    }

    // -------------------------------------------------------------------------
    // Partner image path included when partner exists
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_IncludesPartnerImageWhenPartnerExists()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice", partnerGirlId: 1);
        await SeedUserGirlAsync("user1", 1, bond: 10);

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 25, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("/images/sakura.png", results[0].PartnerImagePath);
    }

    [Fact]
    public async Task SearchUsers_NullPartnerImageWhenNoPartner()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var results = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 25, CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].PartnerImagePath);
    }

    // =========================================================================
    // GetFriendsAsync
    // =========================================================================

    [Fact]
    public async Task GetFriends_ReturnsEmptyWhenNoFriends()
    {
        await SeedUserAsync("user1", "Alice");

        var results = await _query.GetFriendsAsync("user1", CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetFriends_ReturnsFriendsOrderedByDisplayName()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Charlie");
        await SeedUserAsync("user3", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");
        await SeedFriendRelationshipAsync("user1", "user3");

        var results = await _query.GetFriendsAsync("user1", CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Barbara", results[0].DisplayName);
        Assert.Equal("Charlie", results[1].DisplayName);
    }

    [Fact]
    public async Task GetFriends_IncludesPartnerDetailsWhenFriendHasPartner()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara", partnerGirlId: 1);
        await SeedUserGirlAsync("user2", 1, bond: 42);
        await SeedFriendRelationshipAsync("user1", "user2");

        var results = await _query.GetFriendsAsync("user1", CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Sakura", results[0].PartnerName);
        Assert.Equal("/images/sakura.png", results[0].PartnerImagePath);
        Assert.Equal(42, results[0].PartnerBond);
    }

    [Fact]
    public async Task GetFriends_NullPartnerFieldsWhenFriendHasNoPartner()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var results = await _query.GetFriendsAsync("user1", CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].PartnerName);
        Assert.Null(results[0].PartnerImagePath);
        Assert.Null(results[0].PartnerBond);
    }
}
