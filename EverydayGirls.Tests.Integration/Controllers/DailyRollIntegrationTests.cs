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
    /// Integration tests for Daily Roll functionality via HTTP requests to DailyAdoptController.
    /// Tests verify end-to-end flow: HTTP request → Controller → Service → Database → HTTP response.
    /// </summary>
    public sealed class DailyRollIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public DailyRollIntegrationTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        [Fact]
        public async Task UseRoll_WhenAvailable_GeneratesFiveCandidatesAndPersistsState()
        {
            // Arrange - Set time to 2026-01-15 12:00 UTC (before daily reset)
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            
            // Seed 10 girls
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            
            // Create and authenticate test user
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Verify daily state initially has no roll
            var dailyStateBefore = await context.UserDailyStates.FindAsync(user.Id);
            Assert.NotNull(dailyStateBefore);
            Assert.Null(dailyStateBefore.LastDailyRollDate);

            // Act - POST to UseRoll endpoint
            using var formContent = new FormUrlEncodedContent(new Dictionary<string, string>());
            var response = await client.PostAsync("/DailyAdopt/UseRoll", formContent);

            // Assert - HTTP response (should redirect to Index after success)
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/DailyAdopt", location);

            // Assert - Database state changed (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate, dailyStateAfter.LastDailyRollDate);
            Assert.Equal(serverDate, dailyStateAfter.CandidateDate);

            // Assert - 5 candidates persisted
            var candidateIds = new List<int?>
            {
                dailyStateAfter.Candidate1GirlId,
                dailyStateAfter.Candidate2GirlId,
                dailyStateAfter.Candidate3GirlId,
                dailyStateAfter.Candidate4GirlId,
                dailyStateAfter.Candidate5GirlId
            }.Where(id => id.HasValue).ToList();

            Assert.Equal(5, candidateIds.Count);
            Assert.Equal(5, candidateIds.Distinct().Count()); // All unique
        }

        [Fact]
        public async Task UseRoll_WhenAlreadyRolledToday_ReturnsRedirectAndDoesNotChangeState()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // Mark roll as already used today
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate, candidateIds: new[] { 1, 2, 3, 4, 5 });

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            var dailyStateBefore = await context.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            var lastRollBefore = dailyStateBefore.LastDailyRollDate;

            // Act - Attempt second roll same day
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>());
            var response = await client.PostAsync("/DailyAdopt/UseRoll", content);

            // Assert - Redirected (roll blocked)
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);

            // Assert - State unchanged (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(lastRollBefore, dailyStateAfter.LastDailyRollDate);
        }

        [Fact]
        public async Task UseRoll_AfterServerReset_SucceedsWithNewDate()
        {
            // Arrange - Start at 2026-01-15 12:00 UTC
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            var serverDate1 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // Mark roll as used on 2026-01-15
            await IntegrationTestHelpers.UpdateDailyStateAsync(context, user.Id, lastRollDate: serverDate1, candidateIds: new[] { 1, 2, 3, 4, 5 });

            // Act - Advance time past 18:00 UTC reset (now 2026-01-16)
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 19, 0, 0, DateTimeKind.Utc));
            var serverDate2 = DailyCadence.GetServerDateFromUtc(_factory.TestClock.UtcNow);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>());
            var response = await client.PostAsync("/DailyAdopt/UseRoll", content);

            // Assert - Succeeded (redirect after successful roll)
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/DailyAdopt", location);

            // Assert - New server date recorded (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var dailyStateAfter = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            Assert.Equal(serverDate2, dailyStateAfter.LastDailyRollDate);
            Assert.NotEqual(serverDate1, serverDate2);
        }

        [Fact]
        public async Task UseRoll_ExcludesOwnedGirls_FromCandidates()
        {
            // Arrange
            _factory.TestClock.SetUtcNow(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 10);
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            // User already owns girls 1, 2, 3
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 2);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 3);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>());
            var response = await client.PostAsync("/DailyAdopt/UseRoll", content);

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.Redirect || 
                       response.StatusCode == HttpStatusCode.Found ||
                       response.StatusCode == HttpStatusCode.SeeOther,
                       $"Expected redirect but got {response.StatusCode}");

            var location = response.Headers.Location?.ToString() ?? "";
            Assert.DoesNotContain("/Identity/Account/Login", location);
            Assert.Contains("/DailyAdopt", location);

            // Assert - Verify candidates exclude owned girls (use fresh scope)
            using var assertScope = _factory.Services.CreateScope();
            var assertContext = IntegrationTestHelpers.GetDbContext(assertScope.ServiceProvider);
            var dailyState = await assertContext.UserDailyStates.AsNoTracking().FirstAsync(ds => ds.UserId == user.Id);
            var candidateIds = new List<int?>
            {
                dailyState.Candidate1GirlId,
                dailyState.Candidate2GirlId,
                dailyState.Candidate3GirlId,
                dailyState.Candidate4GirlId,
                dailyState.Candidate5GirlId
            }.Where(id => id.HasValue).Select(id => id!.Value).ToList();

            // None of the candidates should be owned girls (1, 2, 3)
            Assert.DoesNotContain(1, candidateIds);
            Assert.DoesNotContain(2, candidateIds);
            Assert.DoesNotContain(3, candidateIds);
        }
    }
}

