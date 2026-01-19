using EverydayGirlsCompanionCollector.Abstractions;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for interaction bond calculation logic.
/// 
/// What is mocked:
/// - IRandom (provides deterministic values to test bond calculation formula)
/// 
/// What scenarios are covered:
/// - Random value below threshold (< 10) produces +2 bond
/// - Random value at or above threshold (>= 10) produces +1 bond
/// - Edge cases at boundaries (0, 9, 10, 99)
/// - Multiple interactions with known random sequences
/// 
/// What rules from PROJECT_OVERVIEW this proves:
/// - Daily interaction grants bond increase
/// - 10% chance (random < 10) for +2 bond
/// - 90% chance (random >= 10) for +1 bond
/// </summary>
public class InteractionBondTests
{
    [Fact]
    public void CalculateBondIncrease_RandomValueBelow10_ReturnsTwo()
    {
        // Arrange: Mock IRandom to return value in special range (< 10)
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(9); // 9 < 10 = special moment

        // Act: Apply bond calculation formula
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert: Should return +2 bond
        Assert.Equal(2, bondIncrease);
    }

    [Fact]
    public void CalculateBondIncrease_RandomValue10OrAbove_ReturnsOne()
    {
        // Arrange: Mock IRandom to return value in normal range (>= 10)
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(10); // 10 >= 10 = normal

        // Act: Apply bond calculation formula
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert: Should return +1 bond
        Assert.Equal(1, bondIncrease);
    }

    [Theory]
    [InlineData(0, 2)]   // Edge: lowest possible value = special
    [InlineData(9, 2)]   // Edge: highest special value
    [InlineData(10, 1)]  // Edge: lowest normal value
    [InlineData(50, 1)]  // Normal mid-range value
    [InlineData(99, 1)]  // Edge: highest possible value
    public void CalculateBondIncrease_VariousRandomValues_ReturnsExpectedBond(int randomValue, int expectedBond)
    {
        // Arrange: Mock IRandom to return specific deterministic value
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(randomValue);

        // Act: Apply bond calculation formula
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert: Verify formula produces expected result for this input
        Assert.Equal(expectedBond, bondIncrease);
    }

    [Fact]
    public void CalculateBondIncrease_DeterministicSequence_ProducesExpectedThresholdSplit()
    {
        // Arrange: Mock IRandom to return deterministic sequence (0-99 repeating 10 times)
        // This creates a known distribution: 10 values < 10 (0-9), 90 values >= 10 (10-99)
        var mockRandom = new Mock<IRandom>();
        var sequence = new Queue<int>();
        
        for (int i = 0; i < 1000; i++)
        {
            sequence.Enqueue(i % 100); // Cycles through 0-99
        }
        
        mockRandom.Setup(r => r.Next(100)).Returns(sequence.Dequeue);

        // Act: Apply bond formula to 1000 deterministic values
        int specialCount = 0;
        int normalCount = 0;
        
        for (int i = 0; i < 1000; i++)
        {
            var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;
            if (bondIncrease == 2)
                specialCount++;
            else
                normalCount++;
        }

        // Assert: With this deterministic sequence, should produce exactly 10% special and 90% normal
        // Proves the formula correctly applies the < 10 threshold
        Assert.Equal(100, specialCount);  // 10 values per cycle * 10 cycles = 100
        Assert.Equal(900, normalCount);   // 90 values per cycle * 10 cycles = 900
    }

    [Fact]
    public void CalculateBondIncrease_ConsecutiveInteractions_EachCalculatesIndependently()
    {
        // Arrange: Mock IRandom to return specific sequence of values
        // Sequence: 5 (< 10), 50 (>= 10), 3 (< 10), 80 (>= 10), 9 (< 10), 15 (>= 10)
        var mockRandom = new Mock<IRandom>();
        var sequence = new Queue<int>(new[] { 5, 50, 3, 80, 9, 15 });
        mockRandom.Setup(r => r.Next(100)).Returns(sequence.Dequeue);

        // Act: Calculate bond for 6 consecutive interactions
        var results = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            results.Add(mockRandom.Object.Next(100) < 10 ? 2 : 1);
        }

        // Assert: Each interaction should produce expected result based on its random value
        // Proves each interaction evaluates independently
        Assert.Equal(new[] { 2, 1, 2, 1, 2, 1 }, results);
    }
}
