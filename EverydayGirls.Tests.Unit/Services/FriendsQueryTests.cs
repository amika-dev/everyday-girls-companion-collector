using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for FriendsQuery (GetFriendsAsync and SearchUsersByDisplayNameAsync).
/// Verifies friend list ordering, search behavior, friendship marking, paging, and summary stats.
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

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "ali", 1, 25, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, r => Assert.StartsWith("ALI", r.DisplayName.ToUpperInvariant()));
    }

    // -------------------------------------------------------------------------
    // Trims input
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_TrimsWhitespaceFromInput()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "  Alice  ", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].DisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchUsers_EmptyOrWhitespaceInput_ReturnsEmptyResult(string? searchText)
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", searchText!, 1, 25, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    // -------------------------------------------------------------------------
    // Excludes self
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_ExcludesSelf()
    {
        await SeedUserAsync("requester", "Alice");
        await SeedUserAsync("user1", "Alicia");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Ali", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("user1", result.Items[0].UserId);
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

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "A", 1, 25, CancellationToken.None);

        var friendResult = result.Items.Single(r => r.UserId == "friend1");
        var strangerResult = result.Items.Single(r => r.UserId == "stranger1");

        Assert.True(friendResult.IsAlreadyFriend);
        Assert.False(friendResult.CanAdd);
        Assert.False(strangerResult.IsAlreadyFriend);
        Assert.True(strangerResult.CanAdd);
    }

    // -------------------------------------------------------------------------
    // Paging: search returns correct TotalCount and page slices
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_ReturnsCorrectTotalCountAndPageSlice()
    {
        await SeedUserAsync("requester", "Requester");
        for (var i = 0; i < 12; i++)
        {
            await SeedUserAsync($"user{i:D2}", $"Alpha{i:D2}");
        }

        var page1 = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 1, 5, CancellationToken.None);
        var page2 = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 2, 5, CancellationToken.None);
        var page3 = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 3, 5, CancellationToken.None);

        Assert.Equal(12, page1.TotalCount);
        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(2, page3.Items.Count);
    }

    // -------------------------------------------------------------------------
    // Paging: page < 1 clamps to 1
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_PageLessThanOneClamps()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alpha");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", -5, 5, CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Single(result.Items);
    }

    // -------------------------------------------------------------------------
    // Paging: pageSize <= 0 uses default
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_PageSizeZeroUsesDefault()
    {
        await SeedUserAsync("requester", "Requester");
        for (var i = 0; i < 10; i++)
        {
            await SeedUserAsync($"user{i}", $"Alpha{i}");
        }

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alpha", 1, 0, CancellationToken.None);

        Assert.Equal(GameConstants.FriendsPageSize, result.PageSize);
        Assert.Equal(GameConstants.FriendsPageSize, result.Items.Count);
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

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("/images/sakura.png", result.Items[0].PartnerImagePath);
    }

    [Fact]
    public async Task SearchUsers_NullPartnerImageWhenNoPartner()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].PartnerImagePath);
    }

    // -------------------------------------------------------------------------
    // CompanionsCount and TotalBond computed correctly for search
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchUsers_IncludesCompanionsCountAndTotalBond()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedGirlAsync(2, "Hana");
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");
        await SeedUserGirlAsync("user1", 1, bond: 10);
        await SeedUserGirlAsync("user1", 2, bond: 5);

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].CompanionsCount);
        Assert.Equal(15, result.Items[0].TotalBond);
    }

    [Fact]
    public async Task SearchUsers_ZeroCompanionsCountAndTotalBondWhenNoCompanions()
    {
        await SeedUserAsync("requester", "Requester");
        await SeedUserAsync("user1", "Alice");

        var result = await _query.SearchUsersByDisplayNameAsync("requester", "Alice", 1, 25, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(0, result.Items[0].CompanionsCount);
        Assert.Equal(0, result.Items[0].TotalBond);
    }

    // =========================================================================
    // GetFriendsAsync
    // =========================================================================

    [Fact]
    public async Task GetFriends_ReturnsEmptyWhenNoFriends()
    {
        await SeedUserAsync("user1", "Alice");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetFriends_ReturnsFriendsOrderedByDisplayName()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Charlie");
        await SeedUserAsync("user3", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");
        await SeedFriendRelationshipAsync("user1", "user3");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Barbara", result.Items[0].DisplayName);
        Assert.Equal("Charlie", result.Items[1].DisplayName);
    }

    [Fact]
    public async Task GetFriends_IncludesPartnerDetailsWhenFriendHasPartner()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara", partnerGirlId: 1);
        await SeedUserGirlAsync("user2", 1, bond: 42);
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("Sakura", result.Items[0].PartnerName);
        Assert.Equal("/images/sakura.png", result.Items[0].PartnerImagePath);
        Assert.Equal(42, result.Items[0].PartnerBond);
    }

    [Fact]
    public async Task GetFriends_NullPartnerFieldsWhenFriendHasNoPartner()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Null(result.Items[0].PartnerName);
        Assert.Null(result.Items[0].PartnerImagePath);
        Assert.Null(result.Items[0].PartnerBond);
    }

    // -------------------------------------------------------------------------
    // Paging: friends list returns correct TotalCount and page slices
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFriends_ReturnsCorrectTotalCountAndPageSlice()
    {
        await SeedUserAsync("user1", "Alice");
        for (var i = 0; i < 7; i++)
        {
            var friendId = $"friend{i:D2}";
            await SeedUserAsync(friendId, $"Friend{i:D2}");
            await SeedFriendRelationshipAsync("user1", friendId);
        }

        var page1 = await _query.GetFriendsAsync("user1", 1, 3, CancellationToken.None);
        var page2 = await _query.GetFriendsAsync("user1", 2, 3, CancellationToken.None);
        var page3 = await _query.GetFriendsAsync("user1", 3, 3, CancellationToken.None);

        Assert.Equal(7, page1.TotalCount);
        Assert.Equal(3, page1.Items.Count);
        Assert.Equal(3, page2.Items.Count);
        Assert.Equal(1, page3.Items.Count);
    }

    // -------------------------------------------------------------------------
    // Paging: page < 1 clamps to 1
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFriends_PageLessThanOneClamps()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _query.GetFriendsAsync("user1", -1, 5, CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Single(result.Items);
    }

    // -------------------------------------------------------------------------
    // Paging: pageSize <= 0 uses default
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFriends_PageSizeZeroUsesDefault()
    {
        await SeedUserAsync("user1", "Alice");
        for (var i = 0; i < 10; i++)
        {
            var friendId = $"friend{i}";
            await SeedUserAsync(friendId, $"Friend{i}");
            await SeedFriendRelationshipAsync("user1", friendId);
        }

        var result = await _query.GetFriendsAsync("user1", 1, 0, CancellationToken.None);

        Assert.Equal(GameConstants.FriendsPageSize, result.PageSize);
        Assert.Equal(GameConstants.FriendsPageSize, result.Items.Count);
    }

    // -------------------------------------------------------------------------
    // CompanionsCount and TotalBond computed correctly for friends
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetFriends_IncludesCompanionsCountAndTotalBond()
    {
        await SeedGirlAsync(1, "Sakura");
        await SeedGirlAsync(2, "Hana");
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedUserGirlAsync("user2", 1, bond: 20);
        await SeedUserGirlAsync("user2", 2, bond: 8);
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].CompanionsCount);
        Assert.Equal(28, result.Items[0].TotalBond);
    }

    [Fact]
    public async Task GetFriends_ZeroCompanionsCountAndTotalBondWhenNoCompanions()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _query.GetFriendsAsync("user1", 1, 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(0, result.Items[0].CompanionsCount);
        Assert.Equal(0, result.Items[0].TotalBond);
    }
}
