using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for DailyRollService.
/// 
/// What is mocked:
/// - IRandom (Shuffle method is tracked/controlled to verify deterministic behavior)
/// 
/// What scenarios are covered:
/// - Candidate generation with sufficient or insufficient girls
/// - Empty input arrays and zero/negative counts
/// - Null input validation
/// - Shuffle is called exactly once before selection
/// - Output contains correct elements after shuffle
/// 
/// What rules from PROJECT_OVERVIEW this proves:
/// - Daily Roll generates N candidates from available pool
/// - Candidates are shuffled before selection (randomization)
/// - Returns all available if pool is smaller than requested count
/// </summary>
public class DailyRollServiceTests
{
    [Fact]
    public void GenerateCandidates_SufficientGirls_ReturnsRequestedCount()
    {
        // Arrange: Mock IRandom to verify shuffle is called
        // DailyRollService should shuffle the input array then return first N elements
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

        // Assert: Should return exactly 5 candidates and call Shuffle exactly once
        Assert.Equal(5, result.Count);
        mockRandom.Verify(r => r.Shuffle(availableGirls), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_InsufficientGirls_ReturnsAllAvailable()
    {
        // Arrange: Create pool smaller than requested count
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" },
            new Girl { GirlId = 2, Name = "Girl2" },
            new Girl { GirlId = 3, Name = "Girl3" }
        };

        // Act: Request 5 candidates when only 3 available
        var result = service.GenerateCandidates(availableGirls, 5);

        // Assert: Should return all 3 available girls and still shuffle
        Assert.Equal(3, result.Count);
        mockRandom.Verify(r => r.Shuffle(availableGirls), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_EmptyArray_ReturnsEmpty()
    {
        // Arrange: Provide empty pool
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        var availableGirls = Array.Empty<Girl>();

        // Act: Request candidates from empty pool
        var result = service.GenerateCandidates(availableGirls, 5);

        // Assert: Should return empty result but still call Shuffle
        Assert.Empty(result);
        mockRandom.Verify(r => r.Shuffle(It.IsAny<Girl[]>()), Times.Once);
    }

    [Fact]
    public void GenerateCandidates_ZeroCount_ReturnsEmpty()
    {
        // Arrange: Request zero candidates
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" }
        };

        // Act: Request 0 candidates
        var result = service.GenerateCandidates(availableGirls, 0);

        // Assert: Should return empty and not shuffle (optimization check)
        Assert.Empty(result);
        mockRandom.Verify(r => r.Shuffle(It.IsAny<Girl[]>()), Times.Never);
    }

    [Fact]
    public void GenerateCandidates_NegativeCount_ReturnsEmpty()
    {
        // Arrange: Provide invalid negative count
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" }
        };

        // Act: Request -1 candidates
        var result = service.GenerateCandidates(availableGirls, -1);

        // Assert: Should return empty (graceful handling of invalid input)
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateCandidates_NullArray_ThrowsArgumentNullException()
    {
        // Arrange: Provide null input
        var mockRandom = new Mock<IRandom>();
        var service = new DailyRollService(mockRandom.Object);

        // Act & Assert: Should throw ArgumentNullException (proper null guard)
        Assert.Throws<ArgumentNullException>(() => service.GenerateCandidates(null!, 5));
    }

    [Fact]
    public void GenerateCandidates_ShuffleCallback_ProducesExpectedOrder()
    {
        // Arrange: Mock IRandom.Shuffle to apply a deterministic transformation (reverse)
        // This proves the service uses the shuffled array for selection
        var mockRandom = new Mock<IRandom>();
        Girl[]? capturedArray = null;
        
        mockRandom.Setup(r => r.Shuffle(It.IsAny<Girl[]>()))
            .Callback<Girl[]>(arr => 
            {
                capturedArray = arr;
                // Apply deterministic transformation: reverse the array
                Array.Reverse(arr);
            });

        var service = new DailyRollService(mockRandom.Object);
        
        var availableGirls = new[]
        {
            new Girl { GirlId = 1, Name = "Girl1" },
            new Girl { GirlId = 2, Name = "Girl2" },
            new Girl { GirlId = 3, Name = "Girl3" }
        };

        // Act: Generate 2 candidates (service should take first 2 after shuffle)
        var result = service.GenerateCandidates(availableGirls, 2);

        // Assert: After reversing [1,2,3] -> [3,2,1], first 2 should be Girl3 and Girl2
        Assert.NotNull(capturedArray);
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].GirlId);  // First element after reverse
        Assert.Equal(2, result[1].GirlId);  // Second element after reverse
    }
}
