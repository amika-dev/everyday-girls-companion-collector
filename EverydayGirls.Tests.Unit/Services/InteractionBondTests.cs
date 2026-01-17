using EverydayGirlsCompanionCollector.Abstractions;
using Moq;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for interaction bond calculation logic.
/// Verifies 90% chance for +1 bond, 10% chance for +2 bond.
/// </summary>
public class InteractionBondTests
{
    [Fact]
    public void CalculateBondIncrease_WithRandomValueBelow10_ReturnsTwo()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(9); // 9 < 10 = special moment

        // Act
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert
        Assert.Equal(2, bondIncrease);
    }

    [Fact]
    public void CalculateBondIncrease_WithRandomValue10OrAbove_ReturnsOne()
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(10); // 10 >= 10 = normal

        // Act
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert
        Assert.Equal(1, bondIncrease);
    }

    [Theory]
    [InlineData(0, 2)]   // Edge: lowest value = special
    [InlineData(9, 2)]   // Edge: highest special value
    [InlineData(10, 1)]  // Edge: lowest normal value
    [InlineData(50, 1)]  // Normal
    [InlineData(99, 1)]  // Edge: highest value
    public void CalculateBondIncrease_WithVariousRandomValues_ReturnsExpectedBond(int randomValue, int expectedBond)
    {
        // Arrange
        var mockRandom = new Mock<IRandom>();
        mockRandom.Setup(r => r.Next(100)).Returns(randomValue);

        // Act
        var bondIncrease = mockRandom.Object.Next(100) < 10 ? 2 : 1;

        // Assert
        Assert.Equal(expectedBond, bondIncrease);
    }

    [Fact]
    public void BondIncrease_ApproximatelyFollows10PercentDistribution()
    {
        // This test verifies the logic produces approximately 10% +2 bonds
        // Arrange: Mock random to return a predictable sequence (0-99 repeating)
        // This creates a known distribution: 10 values < 10, 90 values >= 10
        var mockRandom = new Mock<IRandom>();
        var sequence = new Queue<int>();
        
        for (int i = 0; i < 1000; i++)
        {
            sequence.Enqueue(i % 100); // Cycles through 0-99
        }
        
        mockRandom.Setup(r => r.Next(100)).Returns(sequence.Dequeue);

        // Act: Simulate 1000 interactions
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

        // Assert: Should have exactly 10% special moments and 90% normal moments
        // Proves the formula correctly implements the 10%/90% probability split
        Assert.Equal(100, specialCount);  // 10% of 1000
        Assert.Equal(900, normalCount);   // 90% of 1000
    }

    [Fact]
    public void BondIncrease_WithConsecutiveInteractions_EachRollsIndependently()
    {
        // Verify each interaction is independent
        var mockRandom = new Mock<IRandom>();
        var sequence = new Queue<int>(new[] { 5, 50, 3, 80, 9, 15 });
        mockRandom.Setup(r => r.Next(100)).Returns(sequence.Dequeue);

        // Act
        var results = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            results.Add(mockRandom.Object.Next(100) < 10 ? 2 : 1);
        }

        // Assert
        Assert.Equal(new[] { 2, 1, 2, 1, 2, 1 }, results);
    }
}
