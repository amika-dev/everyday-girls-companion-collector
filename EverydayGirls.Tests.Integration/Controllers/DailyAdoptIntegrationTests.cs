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
    /// Integration tests for Daily Adoption via HTTP requests to DailyAdoptController.
    /// </summary>
    public sealed class DailyAdoptIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public DailyAdoptIntegrationTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public async Task Adopt_WhenAvailable_AddsUserGirlAndMarksAdoptAsUsed()
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

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - POST to Adopt endpoint
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - HTTP response (should redirect after successful adoption)
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            // Assert - UserGirl created
            var adoptedGirl = await context.UserGirls.FirstOrDefaultAsync(ug => ug.UserId == user.Id && ug.GirlId == 1);
            Assert.NotNull(adoptedGirl);
            Assert.Equal(0, adoptedGirl.Bond);

            // Assert - Daily state updated
            var updatedState = await context.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, updatedState.LastDailyAdoptDate);
            Assert.Equal(1, updatedState.TodayAdoptedGirlId);
        }

        [Fact]
        public async Task Adopt_WhenAlreadyAdoptedToday_ReturnsRedirectAndDoesNotAddGirl()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // Already rolled and adopted today
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate, lastAdoptDate: serverDate, candidateIds: new[] { 1, 2, 3, 4, 5 });
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            var collectionCountBefore = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);

            // Act - Attempt second adoption
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=2", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - Redirected (blocked)
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            // Assert - Collection unchanged
            var collectionCountAfter = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(collectionCountBefore, collectionCountAfter);
        }

        [Fact]
        public async Task Adopt_FirstAdoption_SetsPartnerAutomatically()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate, candidateIds: new[] { 1, 2, 3, 4, 5 });
            
            // Verify no partner initially
            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Null(userBefore.PartnerGirlId);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - First adoption
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=1", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var updatedUser = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, updatedUser.PartnerGirlId);
        }

        [Fact]
        public async Task Adopt_SecondAdoption_DoesNotChangePartner()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate1 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // First adoption (day 1)
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate1, candidateIds: new[] { 1, 2, 3, 4, 5 });
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.SetPartnerAsync(context, user.Id, 1);

            // Advance to next day
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));
            var serverDate2 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate2, candidateIds: new[] { 2, 3, 4, 5, 6 });

            var userBefore = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, userBefore.PartnerGirlId);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Second adoption (day 2)
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=2", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - Partner still girl 1
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var updatedUser = await context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
            Assert.Equal(1, updatedUser.PartnerGirlId);
        }

        [Fact]
        public async Task Adopt_WhenCollectionFull_ReturnsRedirectAndDoesNotAddGirl()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 35);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // Fill collection to max (30 girls)
            for (int i = 1; i <= GameConstants.MaxCollectionSize; i++)
            {
                await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, i);
            }

            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate, candidateIds: new[] { 31, 32, 33, 34, 35 });

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act - Attempt to adopt when full
            var response = await client.PostAsync("/DailyAdopt/Adopt?girlId=31", new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert - Blocked
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var collectionCount = await context.UserGirls.CountAsync(ug => ug.UserId == user.Id);
            Assert.Equal(GameConstants.MaxCollectionSize, collectionCount);

            var girl31Adopted = await context.UserGirls.AnyAsync(ug => ug.UserId == user.Id && ug.GirlId == 31);
            Assert.False(girl31Adopted);
        }
    }
}

