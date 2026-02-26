using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for FriendsService.TryAddFriendAsync.
/// Verifies bidirectional friend creation, self-add prevention, duplicate handling, and validation.
///
/// DB: EF Core InMemory provider — fast, no disk I/O, no HTTP.
/// </summary>
public class FriendsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FriendsService _service;

    public FriendsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new FriendsService(_context);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task<ApplicationUser> SeedUserAsync(string userId, string displayName = "TestUser")
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            DisplayName = displayName,
            DisplayNameNormalized = displayName.ToUpperInvariant()
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
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

    // -------------------------------------------------------------------------
    // Success: creates two FriendRelationship rows
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenValid_CreatesTwoBidirectionalRows()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");

        var result = await _service.TryAddFriendAsync("user1", "user2", CancellationToken.None);

        Assert.True(result.Success);

        var rowAtoB = await _context.FriendRelationships
            .FirstOrDefaultAsync(fr => fr.UserId == "user1" && fr.FriendUserId == "user2");
        var rowBtoA = await _context.FriendRelationships
            .FirstOrDefaultAsync(fr => fr.UserId == "user2" && fr.FriendUserId == "user1");

        Assert.NotNull(rowAtoB);
        Assert.NotNull(rowBtoA);
        Assert.Equal(rowAtoB.DateAddedUtc, rowBtoA.DateAddedUtc);
    }

    // -------------------------------------------------------------------------
    // Cannot add self
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenAddingSelf_ReturnsCannotAddSelf()
    {
        await SeedUserAsync("user1", "Alice");

        var result = await _service.TryAddFriendAsync("user1", "user1", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CannotAddSelf", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Cannot add nonexistent user
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenFriendUserDoesNotExist_ReturnsUserNotFound()
    {
        await SeedUserAsync("user1", "Alice");

        var result = await _service.TryAddFriendAsync("user1", "nonexistent", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("UserNotFound", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Cannot add if already friends
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenAlreadyFriends_ReturnsAlreadyFriends()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _service.TryAddFriendAsync("user1", "user2", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AlreadyFriends", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Concurrent duplicate: pre-inserted rows => returns AlreadyFriends, no throw
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenOneRowPreInserted_ReturnsAlreadyFriends()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");

        // Simulate a partial race condition: one direction already inserted.
        _context.FriendRelationships.Add(new FriendRelationship
        {
            UserId = "user1",
            FriendUserId = "user2",
            DateAddedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.TryAddFriendAsync("user1", "user2", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AlreadyFriends", result.ErrorCode);
    }

    [Fact]
    public async Task TryAddFriendAsync_WhenBothRowsPreInserted_ReturnsAlreadyFriends()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _service.TryAddFriendAsync("user1", "user2", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("AlreadyFriends", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Success result has no error fields
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryAddFriendAsync_WhenSuccessful_ReturnsNullErrorFields()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");

        var result = await _service.TryAddFriendAsync("user1", "user2", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    // =========================================================================
    // TryRemoveFriendAsync
    // =========================================================================

    // -------------------------------------------------------------------------
    // Success: removes both directions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryRemoveFriendAsync_WhenFriends_RemovesBothDirections()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _service.TryRemoveFriendAsync("user1", "user2", CancellationToken.None);

        Assert.True(result.Success);

        var anyRemaining = await _context.FriendRelationships
            .AnyAsync(fr =>
                (fr.UserId == "user1" && fr.FriendUserId == "user2") ||
                (fr.UserId == "user2" && fr.FriendUserId == "user1"));

        Assert.False(anyRemaining);
    }

    // -------------------------------------------------------------------------
    // NotFriends when no relationship exists
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryRemoveFriendAsync_WhenNotFriends_ReturnsNotFriends()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");

        var result = await _service.TryRemoveFriendAsync("user1", "user2", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("NotFriends", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Cannot remove self
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryRemoveFriendAsync_WhenSelf_ReturnsCannotRemoveSelf()
    {
        await SeedUserAsync("user1", "Alice");

        var result = await _service.TryRemoveFriendAsync("user1", "user1", CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CannotRemoveSelf", result.ErrorCode);
    }

    // -------------------------------------------------------------------------
    // Race condition: only one row exists
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryRemoveFriendAsync_WhenOnlyOneRowExists_ReturnsSuccessAndRemovesIt()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");

        // Simulate partial state: only one direction exists.
        _context.FriendRelationships.Add(new FriendRelationship
        {
            UserId = "user1",
            FriendUserId = "user2",
            DateAddedUtc = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.TryRemoveFriendAsync("user1", "user2", CancellationToken.None);

        Assert.True(result.Success);

        var anyRemaining = await _context.FriendRelationships
            .AnyAsync(fr =>
                (fr.UserId == "user1" && fr.FriendUserId == "user2") ||
                (fr.UserId == "user2" && fr.FriendUserId == "user1"));

        Assert.False(anyRemaining);
    }

    // -------------------------------------------------------------------------
    // Success result has no error fields
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TryRemoveFriendAsync_WhenSuccessful_ReturnsNullErrorFields()
    {
        await SeedUserAsync("user1", "Alice");
        await SeedUserAsync("user2", "Barbara");
        await SeedFriendRelationshipAsync("user1", "user2");

        var result = await _service.TryRemoveFriendAsync("user1", "user2", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }
}
