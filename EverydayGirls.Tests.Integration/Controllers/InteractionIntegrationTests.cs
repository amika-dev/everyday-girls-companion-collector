using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for Daily Interaction functionality (InteractionController.Do).
    /// </summary>
    public sealed class InteractionIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public InteractionIntegrationTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public async Task Interact_WhenAvailable_IncreasesBondByOne()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);
            _factory.TestRandom.SetFixedValue(50); // >= 10, so +1 bond

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            var partnerBefore = await context.UserGirls.FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(5, partnerBefore.Bond);

            // Act
            var random = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Abstractions.IRandom>();
            var bondIncrease = random.Next(100) < 10 ? 2 : 1;
            partnerBefore.Bond += bondIncrease;

            var dailyState = await context.UserDailyStates.FindAsync(user.Id);
            dailyState!.LastDailyInteractionDate = serverDate;
            await context.SaveChangesAsync();

            // Assert
            var partnerAfter = await context.UserGirls.FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(6, partnerAfter.Bond);
        }

        [Fact]
        public async Task Interact_WithSpecialMoment_IncreasesBondByTwo()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);
            _factory.TestRandom.SetFixedValue(5); // < 10, so +2 bond

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 10);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            var partnerBefore = await context.UserGirls.FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(10, partnerBefore.Bond);

            // Act
            var random = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Abstractions.IRandom>();
            var bondIncrease = random.Next(100) < 10 ? 2 : 1;
            partnerBefore.Bond += bondIncrease;

            var dailyState = await context.UserDailyStates.FindAsync(user.Id);
            dailyState!.LastDailyInteractionDate = serverDate;
            await context.SaveChangesAsync();

            // Assert
            var partnerAfter = await context.UserGirls.FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(12, partnerAfter.Bond); // 10 + 2
        }

        [Fact]
        public async Task Interact_WhenAlreadyInteractedToday_IsBlocked()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastInteractionDate: serverDate);

            // Act
            var dailyStateService = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Services.IDailyStateService>();
            var dailyState = await context.UserDailyStates.FindAsync(user.Id);
            var isAvailable = dailyStateService.IsDailyInteractionAvailable(dailyState!);

            // Assert
            Assert.False(isAvailable);
        }

        [Fact]
        public async Task Interact_WithoutPartner_IsBlocked()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            Assert.Null(user.PartnerGirlId);

            // Act
            var userWithPartner = await context.Users.Include(u => u.Partner).Where(u => u.Id == user.Id).Select(u => u.Partner).FirstOrDefaultAsync();

            // Assert
            Assert.Null(userWithPartner);
        }

        [Fact]
        public async Task Interact_AfterServerReset_BecomesAvailableAgain()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate1 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastInteractionDate: serverDate1);

            var dailyStateService = scope.ServiceProvider.GetRequiredService<EverydayGirlsCompanionCollector.Services.IDailyStateService>();
            var dailyState = await context.UserDailyStates.FindAsync(user.Id);
            Assert.False(dailyStateService.IsDailyInteractionAvailable(dailyState!));

            // Act - Advance past reset
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));
            var serverDate2 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            dailyState = await context.UserDailyStates.FindAsync(user.Id);
            var isAvailable = dailyStateService.IsDailyInteractionAvailable(dailyState!);

            // Assert
            Assert.NotEqual(serverDate1, serverDate2);
            Assert.True(isAvailable);
        }
    }
}
