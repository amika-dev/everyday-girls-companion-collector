using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for Partner Management across multiple controllers via HTTP requests.
    /// Tests verify end-to-end flow: HTTP request ? Controller ? Service ? Database ? HTTP response.
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
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // Set up roll with candidates
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate, candidateIds: new[] { 1, 2, 3, 4, 5 });

            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Null(userBefore.PartnerGirlId);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - First adoption via real HTTP endpoint
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/DailyAdopt", location);

            // Assert - Database state: partner was automatically set (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var updatedUser = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, updatedUser.PartnerGirlId);
        }

        [Fact]
        public async Task SubsequentAdoptions_DoNotChangePartner()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate1 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // First adoption (sets partner)
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate1, candidateIds: new[] { 1, 2, 3, 4, 5 });
            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);
            await client.PostAsync("/DailyAdopt/Adopt?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userBefore.PartnerGirlId);

            // Advance to next day
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));
            var serverDate2 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate2, candidateIds: new[] { 2, 3, 4, 5, 6 });

            // Act - Second adoption via real HTTP endpoint
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=2", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/DailyAdopt", location);

            // Assert - Database state: partner unchanged (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userAfter.PartnerGirlId); // Still girl 1
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

            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userBefore.PartnerGirlId);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Set partner via real HTTP endpoint
            var response = await client.PostAsync("/Collection/SetPartner?girlId=2", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Collection", location);

            // Assert - Database state: partner changed to girl 2 (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(2, userAfter.PartnerGirlId);
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

            var collectionBefore = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(2, collectionBefore);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Attempt to abandon partner via real HTTP endpoint
            var response = await client.PostAsync("/Collection/Abandon?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response (redirected, operation blocked)
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - Database unchanged: girl 1 still in collection (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var girl1Exists = await assertContext.UserGirls.AsNoTracking().AnyAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.True(girl1Exists);

            var collectionAfter = await assertContext.UserGirls.AsNoTracking().CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(2, collectionAfter);

            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userAfter.PartnerGirlId); // Partner unchanged
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

            var collectionBefore = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(3, collectionBefore);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Abandon non-partner girl via real HTTP endpoint
            var response = await client.PostAsync("/Collection/Abandon?girlId=2", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Collection", location);

            // Assert - Database state: girl 2 removed, partner and girl 1/3 remain (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var girl2Exists = await assertContext.UserGirls.AsNoTracking().AnyAsync(ug => ug.UserId == user.Id && ug.GirlId == 2);
            Assert.False(girl2Exists);

            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userAfter.PartnerGirlId); // Partner unchanged

            var collectionAfter = await assertContext.UserGirls.AsNoTracking().CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(2, collectionAfter);
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

            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Null(userBefore.PartnerGirlId);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Attempt to interact without partner via real HTTP endpoint
            var response = await client.PostAsync("/Interaction/Do", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response (redirected, operation blocked)
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - Database unchanged: user still has no partner (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Null(userAfter.PartnerGirlId);
        }

        [Fact]
        public async Task InteractionWithPartner_Succeeds()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            _factory.TestRandom.SetFixedValue(50); // +1 bond

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            var userBefore = await context.Users.AsNoTracking().Include(u => u.Partner).FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userBefore.PartnerGirlId);
            Assert.NotNull(userBefore.Partner);

            var bondBefore = (await context.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;
            Assert.Equal(5, bondBefore);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Interact with partner via real HTTP endpoint
            var response = await client.PostAsync("/Interaction/Do", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Interaction", location);

            // Assert - Database state: bond increased, daily state marked as used (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var bondAfter = (await assertContext.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;
            Assert.Equal(6, bondAfter); // 5 + 1

            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, dailyStateAfter.LastDailyInteractionDate);
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

            // User1 adopts girl 1 and sets as partner
            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1);
            await IntegrationTestHelpers.SetPartnerAsync(context, user1.Id, 1);

            // User2 adopts girl 2 and sets as partner
            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 2);
            await IntegrationTestHelpers.SetPartnerAsync(context, user2.Id, 2);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user2.Id, user2.Email!); // Authenticate as user2

            // Act - User2 attempts to set partner to girl 1 (owned by user1) via real HTTP endpoint
            var response = await client.PostAsync("/Collection/SetPartner?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response (redirected, operation blocked)
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - Database unchanged: ownership isolation maintained (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var user1OwnsGirl1 = await assertContext.UserGirls.AsNoTracking().AnyAsync(ug => ug.UserId == user1.Id && ug.GirlId == 1);
            var user2OwnsGirl1 = await assertContext.UserGirls.AsNoTracking().AnyAsync(ug => ug.UserId == user2.Id && ug.GirlId == 1);
            Assert.True(user1OwnsGirl1);
            Assert.False(user2OwnsGirl1);

            var user1After = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user1.Id);
            var user2After = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user2.Id);
            Assert.Equal(1, user1After.PartnerGirlId); // User1 partner unchanged
            Assert.Equal(2, user2After.PartnerGirlId); // User2 partner unchanged
        }
    }
}
