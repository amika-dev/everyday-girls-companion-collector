using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Tests to verify integration test infrastructure is working correctly.
    /// </summary>
    public sealed class InfrastructureTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public InfrastructureTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public void Factory_CanCreateAndAccessDatabase()
        {
            // Arrange & Act
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);

            // Assert
            Assert.NotNull(context);
            Assert.NotNull(context.Database);
        }

        [Fact]
        public async Task TestClock_IsInjectedAndControllable()
        {
            // Arrange
            var expectedTime = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
            
            // Act
            using var scope = _factory.Services.CreateScope();
            var clock = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Abstractions.IClock>();

            // Assert
            Assert.Equal(expectedTime, clock.UtcNow);
            Assert.Equal(expectedTime, _factory.TestClock.UtcNow);
        }

        [Fact]
        public async Task TestRandom_IsInjectedAndDeterministic()
        {
            // Arrange & Act
            using var scope = _factory.Services.CreateScope();
            var random = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Abstractions.IRandom>();

            var value1 = random.Next(100);
            var value2 = random.Next(100);
            var value3 = random.Next(100);

            // Assert
            // TestRandom is seeded with 0-99 sequence
            Assert.Equal(0, value1);
            Assert.Equal(1, value2);
            Assert.Equal(2, value3);
        }

        [Fact]
        public async Task Database_CanSeedAndQueryGirls()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);

            // Get current count before seeding
            var existingCount = await context.Girls.CountAsync();

            // Act
            await IntegrationTestHelpers.SeedGirlsAsync(context, 5);
            var girls = await context.Girls.ToListAsync();

            // Assert
            // Should have at least the 5 we just seeded (may have more from other tests)
            Assert.True(girls.Count >= existingCount + 5);
        }

        [Fact]
        public async Task Database_CanCreateUser()
        {
            // Arrange & Act
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);

            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider, "testuser-unique@example.com");

            // Assert
            Assert.NotNull(user);
            Assert.NotNull(user.Id);
            Assert.Equal("testuser-unique@example.com", user.Email);

            // Verify daily state was created
            var dailyState = await context.UserDailyStates.FindAsync(user.Id);
            Assert.NotNull(dailyState);
        }
    }
}
