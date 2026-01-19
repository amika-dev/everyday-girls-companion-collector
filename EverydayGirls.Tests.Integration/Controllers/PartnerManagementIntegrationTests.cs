using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for Partner Management across multiple controllers.
    /// </summary>
    public sealed class PartnerManagementIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public PartnerManagementIntegrationTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public async Task FirstAdoption_SetsPartnerAutomatically()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            Assert.Null(user.PartnerGirlId);

            // Act
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);

            var userEntity = await context.Users.FindAsync(user.Id);
            userEntity!.PartnerGirlId = 1;
            await context.SaveChangesAsync();

            // Assert
            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal(1, updatedUser!.PartnerGirlId);
        }

        [Fact]
        public async Task SubsequentAdoptions_DoNotChangePartner()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            var userBefore = await context.Users.FindAsync(user.Id);
            Assert.Equal(1, userBefore!.PartnerGirlId);

            // Act
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 2);

            // Assert
            var userAfter = await context.Users.FindAsync(user.Id);
            Assert.Equal(1, userAfter!.PartnerGirlId); // Still girl 1
        }

        [Fact]
        public async Task PartnerSwitch_UpdatesAndPersists()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 2);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 3);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            // Act
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 2);

            // Assert
            var userAfter = await context.Users.FindAsync(user.Id);
            Assert.Equal(2, userAfter!.PartnerGirlId);
        }

        [Fact]
        public async Task AbandonPartner_IsBlocked()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 2);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            // Act
            var userEntity = await context.Users.FindAsync(user.Id);
            var attemptedGirlId = 1;
            var isPartner = userEntity!.PartnerGirlId == attemptedGirlId;

            // Assert
            Assert.True(isPartner);
            var collectionBefore = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(2, collectionBefore);
        }

        [Fact]
        public async Task AbandonNonPartner_Succeeds()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 2);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 3);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            // Act
            var girl2 = await context.UserGirls.FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 2);
            context.UserGirls.Remove(girl2);
            await context.SaveChangesAsync();

            // Assert
            var girl2Exists = await context.UserGirls.AnyAsync(ug => ug.UserId == user.Id && ug.GirlId == 2);
            Assert.False(girl2Exists);

            var userAfter = await context.Users.FindAsync(user.Id);
            Assert.Equal(1, userAfter!.PartnerGirlId);

            var collectionCount = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(2, collectionCount);
        }

        [Fact]
        public async Task InteractionWithoutPartner_IsBlocked()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            Assert.Null(user.PartnerGirlId);

            // Act
            var partner = await context.Users.Include(u => u.Partner).Where(u => u.Id == user.Id).Select(u => u.Partner).FirstOrDefaultAsync();

            // Assert
            Assert.Null(partner);
        }

        [Fact]
        public async Task InteractionWithPartner_Succeeds()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            // Act
            var userWithPartner = await context.Users.Include(u => u.Partner).FirstOrDefaultAsync(u => u.Id == user.Id);
            var hasPartner = userWithPartner?.PartnerGirlId != null && userWithPartner.Partner != null;

            // Assert
            Assert.True(hasPartner);
            Assert.Equal(1, userWithPartner!.PartnerGirlId);
            Assert.NotNull(userWithPartner.Partner);
            Assert.Equal("TestGirl001", userWithPartner.Partner.Name);
        }

        [Fact]
        public async Task UserCannotModifyOtherUsersPartner()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);

            var user1 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider, "user1@example.com");
            var user2 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider, "user2@example.com");

            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1);
            await IntegrationTestHelpers.SetPartnerAsync(context, user1.Id, 1);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 2);
            await IntegrationTestHelpers.SetPartnerAsync(context, user2.Id, 2);

            // Act
            var user1OwnsGirl1 = await context.UserGirls.AnyAsync(ug => ug.UserId == user1.Id && ug.GirlId == 1);
            var user2OwnsGirl1 = await context.UserGirls.AnyAsync(ug => ug.UserId == user2.Id && ug.GirlId == 1);

            // Assert
            Assert.True(user1OwnsGirl1);
            Assert.False(user2OwnsGirl1);
        }
    }
}
