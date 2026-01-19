using EverydayGirlsCompanionCollector.Services;
using Xunit;

namespace EverydayGirls.Tests.Unit.Services;

/// <summary>
/// Tests for AdoptionService.
/// Verifies adoption rules: max 30 collection size, first adopt sets partner.
/// 
/// NO MOCKS: AdoptionService has pure business logic with no external dependencies.
/// 
/// These tests prove:
/// - Collection size limit (30) is enforced correctly
/// - First adoption (null partner) correctly returns true for ShouldSetAsPartner
/// - Subsequent adoptions do not override the partner
/// - Edge cases (at limit, over limit, negative sizes) are handled
/// </summary>
public class AdoptionServiceTests
{
    private readonly AdoptionService _service;

    public AdoptionServiceTests()
    {
        _service = new AdoptionService();
    }

    [Theory]
    [InlineData(0, 30, true)]   // Empty collection
    [InlineData(1, 30, true)]   // One girl
    [InlineData(29, 30, true)]  // One slot remaining
    [InlineData(30, 30, false)] // Full collection
    [InlineData(31, 30, false)] // Over limit (edge case)
    public void CanAdopt_WithVariousCollectionSizes_ReturnsExpectedResult(int currentSize, int maxSize, bool expected)
    {
        // Act
        var result = _service.CanAdopt(currentSize, maxSize);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CanAdopt_WhenExactlyAtLimit_ReturnsFalse()
    {
        // Arrange
        const int maxSize = 30;
        const int currentSize = 30;

        // Act
        var result = _service.CanAdopt(currentSize, maxSize);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanAdopt_WhenOneBelowLimit_ReturnsTrue()
    {
        // Arrange
        const int maxSize = 30;
        const int currentSize = 29;

        // Act
        var result = _service.CanAdopt(currentSize, maxSize);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAdopt_WithNegativeCollectionSize_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.CanAdopt(-1, 30));
        Assert.Contains("Collection size cannot be negative", ex.Message);
    }

    [Fact]
    public void CanAdopt_WithZeroMaxSize_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.CanAdopt(0, 0));
        Assert.Contains("Max size must be positive", ex.Message);
    }

    [Fact]
    public void CanAdopt_WithNegativeMaxSize_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.CanAdopt(0, -1));
        Assert.Contains("Max size must be positive", ex.Message);
    }

    [Fact]
    public void ShouldSetAsPartner_WhenNoCurrentPartner_ReturnsTrue()
    {
        // Act
        var result = _service.ShouldSetAsPartner(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldSetAsPartner_WhenPartnerExists_ReturnsFalse()
    {
        // Act
        var result = _service.ShouldSetAsPartner(1);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void ShouldSetAsPartner_WithAnyPartnerId_ReturnsFalse(int partnerId)
    {
        // Act
        var result = _service.ShouldSetAsPartner(partnerId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AdoptionRules_FirstAdoption_SetsPartnerAndAllowsAdoption()
    {
        // Scenario: User's first adoption
        // Arrange
        int collectionSize = 0;
        int? currentPartnerId = null;

        // Act
        var canAdopt = _service.CanAdopt(collectionSize, 30);
        var shouldSetPartner = _service.ShouldSetAsPartner(currentPartnerId);

        // Assert
        Assert.True(canAdopt, "First adoption should be allowed");
        Assert.True(shouldSetPartner, "First adoption should set partner");
    }

    [Fact]
    public void AdoptionRules_SecondAdoption_AllowsAdoptionButDoesNotSetPartner()
    {
        // Scenario: User already has one girl and a partner
        // Arrange
        int collectionSize = 1;
        int? currentPartnerId = 1;

        // Act
        var canAdopt = _service.CanAdopt(collectionSize, 30);
        var shouldSetPartner = _service.ShouldSetAsPartner(currentPartnerId);

        // Assert
        Assert.True(canAdopt, "Second adoption should be allowed");
        Assert.False(shouldSetPartner, "Second adoption should not set partner");
    }

    [Fact]
    public void AdoptionRules_At30Girls_BlocksAdoption()
    {
        // Scenario: Collection is full
        // Arrange
        int collectionSize = 30;
        int? currentPartnerId = 5;

        // Act
        var canAdopt = _service.CanAdopt(collectionSize, 30);
        var shouldSetPartner = _service.ShouldSetAsPartner(currentPartnerId);

        // Assert
        Assert.False(canAdopt, "Adoption should be blocked at max size");
        Assert.False(shouldSetPartner, "Should not set partner when adoption blocked");
    }
}
