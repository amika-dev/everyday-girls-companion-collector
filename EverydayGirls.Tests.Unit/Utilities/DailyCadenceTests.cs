using EverydayGirlsCompanionCollector.Utilities;
using Xunit;

namespace EverydayGirls.Tests.Unit.Utilities;

/// <summary>
/// Tests for DailyCadence utility class.
/// Verifies server-day calculation based on 18:00 UTC reset rule.
/// 
/// NO MOCKS: DailyCadence is a static utility class with pure functions.
/// 
/// These tests prove that:
/// - Server date correctly shifts at 18:00 UTC (before 18:00 = previous day, at/after 18:00 = current day)
/// - Days-since-adoption counts server day transitions, not wall-clock days
/// - Edge cases (midnight, month boundaries) are handled correctly
/// </summary>
public class DailyCadenceTests
{
    [Theory]
    [InlineData("2025-01-15T17:59:59Z", "2025-01-14")] // Just before reset - yesterday
    [InlineData("2025-01-15T18:00:00Z", "2025-01-15")] // At reset time - today
    [InlineData("2025-01-15T18:00:01Z", "2025-01-15")] // Just after reset - today
    [InlineData("2025-01-15T23:59:59Z", "2025-01-15")] // End of day - today
    [InlineData("2025-01-15T00:00:00Z", "2025-01-14")] // Start of day - yesterday
    [InlineData("2025-01-15T12:00:00Z", "2025-01-14")] // Noon - yesterday
    public void GetServerDateFromUtc_ReturnsCorrectServerDate(string utcTimeString, string expectedDateString)
    {
        // Arrange
        var utcTime = DateTime.Parse(utcTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var expectedDate = DateOnly.Parse(expectedDateString);

        // Act
        var result = DailyCadence.GetServerDateFromUtc(utcTime);

        // Assert
        Assert.Equal(expectedDate, result);
    }

    [Fact]
    public void GetDaysSinceAdoption_WhenAdoptedSameServerDay_ReturnsZero()
    {
        // Arrange
        var currentServerDate = new DateOnly(2025, 1, 15);
        var dateMetUtc = new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc); // 19:00 UTC = server date 2025-01-15

        // Act
        var result = DailyCadence.GetDaysSinceAdoption(currentServerDate, dateMetUtc);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetDaysSinceAdoption_WhenAdoptedYesterday_ReturnsOne()
    {
        // Arrange
        var currentServerDate = new DateOnly(2025, 1, 16);
        var dateMetUtc = new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc); // Server date 2025-01-15

        // Act
        var result = DailyCadence.GetDaysSinceAdoption(currentServerDate, dateMetUtc);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetDaysSinceAdoption_WhenAdoptedBeforeResetOnSameDay_HandlesCorrectly()
    {
        // Arrange
        var currentServerDate = new DateOnly(2025, 1, 15);
        var dateMetUtc = new DateTime(2025, 1, 15, 17, 30, 0, DateTimeKind.Utc); // 17:30 UTC = server date 2025-01-14

        // Act
        var result = DailyCadence.GetDaysSinceAdoption(currentServerDate, dateMetUtc);

        // Assert
        Assert.Equal(1, result); // One server day has passed
    }

    [Fact]
    public void GetDaysSinceAdoption_WhenAdoptedSevenDaysAgo_ReturnsSeven()
    {
        // Arrange
        var currentServerDate = new DateOnly(2025, 1, 22);
        var dateMetUtc = new DateTime(2025, 1, 15, 19, 0, 0, DateTimeKind.Utc); // Server date 2025-01-15

        // Act
        var result = DailyCadence.GetDaysSinceAdoption(currentServerDate, dateMetUtc);

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void GetDaysSinceAdoption_CrossingMonthBoundary_CalculatesCorrectly()
    {
        // Arrange
        var currentServerDate = new DateOnly(2025, 2, 3);
        var dateMetUtc = new DateTime(2025, 1, 30, 20, 0, 0, DateTimeKind.Utc); // Server date 2025-01-30

        // Act
        var result = DailyCadence.GetDaysSinceAdoption(currentServerDate, dateMetUtc);

        // Assert
        Assert.Equal(4, result); // Jan 31, Feb 1, Feb 2, Feb 3 = 4 server days
    }
}
