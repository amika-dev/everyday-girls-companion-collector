using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for DailyStateService.
/// Verifies server date calculation, reset time logic, and action availability checks.
/// </summary>
public class DailyStateServiceTests
{
    private readonly Mock<IClock> _mockClock;
    private readonly DailyStateService _service;

    public DailyStateServiceTests()
    {
        _mockClock = new Mock<IClock>();
        _service = new DailyStateService(_mockClock.Object);
    }

    [Theory]
    [InlineData("2025-01-15T17:59:59Z", "2025-01-14")] // Before reset
    [InlineData("2025-01-15T18:00:00Z", "2025-01-15")] // At reset
    [InlineData("2025-01-15T18:00:01Z", "2025-01-15")] // After reset
    [InlineData("2025-01-15T23:59:59Z", "2025-01-15")] // Late evening
    [InlineData("2025-01-15T00:00:00Z", "2025-01-14")] // Midnight
    public void GetCurrentServerDate_ReturnsCorrectDate(string utcTimeString, string expectedDateString)
    {
        // Arrange
        var utcTime = DateTime.Parse(utcTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var expectedDate = DateOnly.Parse(expectedDateString);
        _mockClock.Setup(c => c.UtcNow).Returns(utcTime);

        // Act
        var result = _service.GetCurrentServerDate();

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Theory]
    [InlineData("2025-01-15T17:59:59Z", 0, 0, 1)] // 1 second until reset
    [InlineData("2025-01-15T17:00:00Z", 1, 0, 0)] // 1 hour until reset
    [InlineData("2025-01-15T18:00:01Z", 23, 59, 59)] // Just after reset - almost 24 hours
    [InlineData("2025-01-15T12:00:00Z", 6, 0, 0)] // 6 hours until reset
    public void GetTimeUntilReset_ReturnsCorrectTimeSpan(string utcTimeString, int expectedHours, int expectedMinutes, int expectedSeconds)
    {
        // Arrange
        var utcTime = DateTime.Parse(utcTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        _mockClock.Setup(c => c.UtcNow).Returns(utcTime);

        // Act
        var result = _service.GetTimeUntilReset();

        // Assert
        Assert.Equal(expectedHours, result.Hours);
        Assert.Equal(expectedMinutes, result.Minutes);
        Assert.Equal(expectedSeconds, result.Seconds);
    }

    [Fact]
    public void IsDailyRollAvailable_WhenNeverUsed_ReturnsTrue()
    {
        // Arrange: Mock clock at a known time
        // UserDailyState has LastDailyRollDate = null (never used)
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyRollDate = null
        };

        // Act: Check if daily roll is available
        var result = _service.IsDailyRollAvailable(state);

        // Assert: First roll of the day should be available
        // Proves null LastDailyRollDate is treated as "never used"
        Assert.True(result);
    }

    [Fact]
    public void IsDailyRollAvailable_WhenUsedOnDifferentServerDate_ReturnsTrue()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 16, 19, 0, 0, DateTimeKind.Utc)); // Server date: Jan 16
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyRollDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyRollAvailable(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDailyRollAvailable_WhenUsedOnSameServerDate_ReturnsFalse()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc)); // Server date: Jan 15
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyRollDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyRollAvailable(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDailyRollAvailable_ThrowsOnNullState()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsDailyRollAvailable(null!));
    }

    [Fact]
    public void IsDailyAdoptAvailable_WhenNeverUsed_ReturnsTrue()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyAdoptDate = null
        };

        // Act
        var result = _service.IsDailyAdoptAvailable(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDailyAdoptAvailable_WhenUsedOnDifferentServerDate_ReturnsTrue()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 16, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyAdoptDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyAdoptAvailable(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDailyAdoptAvailable_WhenUsedOnSameServerDate_ReturnsFalse()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyAdoptDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyAdoptAvailable(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDailyAdoptAvailable_ThrowsOnNullState()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsDailyAdoptAvailable(null!));
    }

    [Fact]
    public void IsDailyInteractionAvailable_WhenNeverUsed_ReturnsTrue()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyInteractionDate = null
        };

        // Act
        var result = _service.IsDailyInteractionAvailable(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDailyInteractionAvailable_WhenUsedOnDifferentServerDate_ReturnsTrue()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 16, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyInteractionDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyInteractionAvailable(state);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDailyInteractionAvailable_WhenUsedOnSameServerDate_ReturnsFalse()
    {
        // Arrange
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc));
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyInteractionDate = new DateOnly(2025, 1, 15)
        };

        // Act
        var result = _service.IsDailyInteractionAvailable(state);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDailyInteractionAvailable_ThrowsOnNullState()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsDailyInteractionAvailable(null!));
    }

    [Fact]
    public void IsDailyRollAvailable_BeforeAndAfterReset_ReturnsCorrectly()
    {
        // Arrange
        var state = new UserDailyState
        {
            UserId = "user1",
            LastDailyRollDate = new DateOnly(2025, 1, 15)
        };

        // Before reset - still server date Jan 15
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 16, 17, 30, 0, DateTimeKind.Utc));
        var resultBefore = _service.IsDailyRollAvailable(state);

        // After reset - now server date Jan 16
        _mockClock.Setup(c => c.UtcNow).Returns(new DateTime(2025, 1, 16, 18, 30, 0, DateTimeKind.Utc));
        var resultAfter = _service.IsDailyRollAvailable(state);

        // Assert
        Assert.False(resultBefore); // Same server date
        Assert.True(resultAfter);   // Different server date
    }
}
