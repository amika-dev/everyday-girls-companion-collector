using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Enums;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for Daily Interaction functionality via HTTP requests to InteractionController.
    /// Tests verify end-to-end flow: HTTP request → Controller → Service → Database → HTTP response.
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

            var partnerBefore = await context.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(5, partnerBefore.Bond);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - POST to Interaction/Do endpoint
            using var content = new FormUrlEncodedContent([]);
            var response = await client.PostAsync("/Interaction/Do", content);


            // Assert - HTTP response should redirect to Interaction/Index
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");
            
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Interaction", location);

            // Assert - Database state changed: bond increased by 1, daily state marked as used (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var partnerAfter = await assertContext.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(6, partnerAfter.Bond); // 5 + 1

            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, dailyStateAfter.LastDailyInteractionDate);
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

            var partnerBefore = await context.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(10, partnerBefore.Bond);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - POST to Interaction/Do endpoint
            using var content = new FormUrlEncodedContent([]);
            var response = await client.PostAsync("/Interaction/Do", content);

            // Assert - HTTP response should redirect
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Interaction", location);

            // Assert - Database state changed: bond increased by 2, daily state marked as used (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var partnerAfter = await assertContext.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.Equal(12, partnerAfter.Bond); // 10 + 2

            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, dailyStateAfter.LastDailyInteractionDate);
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

            var bondBefore = (await context.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Attempt to interact again
            using var content = new FormUrlEncodedContent([]);
            var response = await client.PostAsync("/Interaction/Do", content);

            // Assert - HTTP response should redirect (blocked)
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - Database unchanged: bond not increased, daily state still marked as used (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var bondAfter = (await assertContext.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;
            Assert.Equal(bondBefore, bondAfter); // No change

            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, dailyStateAfter.LastDailyInteractionDate);
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

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Attempt to interact without partner
            using var content = new FormUrlEncodedContent([]);
            var response = await client.PostAsync("/Interaction/Do", content);

            // Assert - HTTP response should redirect (blocked)
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect || 
                response.StatusCode == HttpStatusCode.Found ||
                response.StatusCode == HttpStatusCode.SeeOther,
                $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - User still has no partner (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var userAfter = await assertContext.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Null(userAfter.PartnerGirlId);
        }

        [Fact]
        public async Task Interact_AfterServerReset_BecomesAvailableAgain()
        {
            // Arrange - Interact once before reset
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate1 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 5);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            _factory.TestRandom.SetFixedValue(50); // +1 bond

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // First interaction (before reset)
            using var content1 = new FormUrlEncodedContent([]);
            var response1 = await client.PostAsync("/Interaction/Do", content1);
            Assert.True(
                response1.StatusCode == HttpStatusCode.Redirect || 
                response1.StatusCode == HttpStatusCode.Found ||
                response1.StatusCode == HttpStatusCode.SeeOther);

            // Verify first interaction succeeded (use fresh scope)
            using (var assertScope1 = _factory.Services.CreateScope())
            {
                var assertContext1 = IntegrationTestHelpers.GetDbContext(assertScope1.ServiceProvider);
                var bondAfterFirst = (await assertContext1.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;
                Assert.Equal(6, bondAfterFirst); // 5 + 1
            }

            // Act - Advance past reset (18:00 UTC)
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));
            var serverDate2 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);
            Assert.NotEqual(serverDate1, serverDate2);

            // Second interaction (after reset)
            using var content2 = new FormUrlEncodedContent([]);
            var response2 = await client.PostAsync("/Interaction/Do", content2);

            // Assert - Second interaction succeeded
            Assert.True(
                response2.StatusCode == HttpStatusCode.Redirect || 
                response2.StatusCode == HttpStatusCode.Found ||
                response2.StatusCode == HttpStatusCode.SeeOther);

            var location = response2.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/Interaction", location);

            // Assert - Bond increased again (use fresh scope)
            using var assertScope2 = _factory.Services.CreateScope();
            var assertContext2 = IntegrationTestHelpers.GetDbContext(assertScope2.ServiceProvider);
            var bondAfterSecond = (await assertContext2.UserGirls.AsNoTracking().FirstAsync(ug => ug.UserId == user.Id && ug.GirlId == 1)).Bond;
            Assert.Equal(7, bondAfterSecond); // 6 + 1

            var dailyStateAfter = await assertContext2.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate2, dailyStateAfter.LastDailyInteractionDate);
        }
    }
}
