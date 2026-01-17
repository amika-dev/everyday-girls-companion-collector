using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for DailyRollService.
/// Verifies candidate generation logic.
/// </summary>
public class DailyRollServiceTests
{
    [Fact]
    public void GenerateCandidates_WithEnoughGirls_ReturnsRequestedCount()
    {
        // Arrange: Mock IRandom to track shuffle calls
        // Service should shuffle the array then take N items
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" },
            new Girl { GirlId = 2, Name = "Girl2" },
            new Girl { GirlId = 3, Name = "Girl3" },
            new Girl { GirlId = 4, Name = "Girl4" },
            new Girl { GirlId = 5, Name = "Girl5" },
            new Girl { GirlId = 6, Name = "Girl6" },
            new Girl { GirlId = 7, Name = "Girl7" }
        };

        // Act: Generate 5 candidates from 7 available girls
        var result = service.GenerateCandidates(availableGirls, 5);

        // Assert: Should return exactly 5 candidates and shuffle exactly once
        // Proves the service respects the count parameter and shuffles before selection
        Assert.Equal(5, result.Count);
        mockRandom.Verify(r => r.Shuffle(availableGirls), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_WithFewerGirlsThanRequested_ReturnsAllAvailable()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" },
            new Girl { GirlId = 2, Name = "Girl2" },
            new Girl { GirlId = 3, Name = "Girl3" }
        };

        // Act
        var result = service.GenerateCandidates(availableGirls, 5);

        // Assert
        Assert.Equal(3, result.Count);
        mockRandom.Verify(r => r.Shuffle(availableGirls), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_WithEmptyArray_ReturnsEmpty()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        var availableGirls = Array.Empty<Girl>();

        // Act
        var result = service.GenerateCandidates(availableGirls, 5);

        // Assert
        Assert.Empty(result);
        mockRandom.Verify(r => r.Shuffle(It.IsAny<Girl[]>()), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_WithZeroCount_ReturnsEmpty()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" }
        };

        // Act
        var result = service.GenerateCandidates(availableGirls, 0);

        // Assert
        Assert.Empty(result);
        mockRandom.Verify(r => r.Shuffle(It.IsAny<Girl[]>()), Times.Never);
    }

    [Fact]
    public void GenerateCandidates_WithNegativeCount_ReturnsEmpty()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" }
        };

        // Act
        var result = service.GenerateCandidates(availableGirls, -1);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateCandidates_WithNullArray_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.GenerateCandidates(null!, 5));
    }

    [Fact]
    public void GenerateCandidates_ShufflesArrayBeforeSelection()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        Girl[]? capturedArray = null;
        
        mockRandom.Setup(r => r.Shuffle(It.IsAny<Girl[]>()))
            .Callback<Girl[]>(arr => 
            {
                capturedArray = arr;
                // Simulate shuffle by reversing
                Array.Reverse(arr);
            });

        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" },
            new Girl { GirlId = 2, Name = "Girl2" },
            new Girl { GirlId = 3, Name = "Girl3" }
        };

        // Act
        var result = service.GenerateCandidates(availableGirls, 2);

        // Assert
        Assert.NotNull(capturedArray);
        Assert.Equal(2, result.Count);
        // After reverse, first 2 should be Girl3 and Girl2
        Assert.Equal(3, result[0].GirlId);
        Assert.Equal(2, result[1].GirlId);
    }
}
