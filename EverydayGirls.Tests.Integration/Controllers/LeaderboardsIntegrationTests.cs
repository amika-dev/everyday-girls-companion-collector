using EverydayGirls.Tests.Integration.Infrastructure;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace EverydayGirls.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for GET /Leaderboards (TotalBond and CompanionBond).
    /// Verifies the full HTTP → Controller → LeaderboardQuery → View rendering path
    /// using SQLite in-memory via TestWebApplicationFactory.
    ///
    /// Hard constraints:
    /// - No new product behaviour is introduced.
    /// - No bond/progression logic is altered.
    /// - Tests are read-only assertions against the leaderboard recognition layer.
    /// </summary>
    public sealed class LeaderboardsIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory _factory;

        public LeaderboardsIntegrationTests()
        {
            _factory = new TestWebApplicationFactory();
        }

        public void Dispose()
        {
            _factory.Dispose();
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        /// <summary>
        /// Seeds a single Girl record with the given ID and optional image URL.
        /// Bypasses SeedGirlsAsync so tests can control GirlId and ImageUrl precisely.
        /// </summary>
        private static async Task SeedGirlAsync(ApplicationDbContext context, int girlId, string name, string? imageUrl)
        {
            // Empty string represents "no image" — the view uses IsNullOrEmpty to decide between
            // the portrait <img> and the letter-circle fallback.
            context.Girls.Add(new Girl { GirlId = girlId, Name = name, ImageUrl = imageUrl ?? string.Empty });
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Sets the DisplayName and DisplayNameNormalized for a user that was created
        /// via CreateTestUserAsync (which generates a default display name).
        /// </summary>
        private static async Task SetDisplayNameAsync(ApplicationDbContext context, string userId, string displayName)
        {
            var user = await context.Users.FindAsync(userId);
            user!.DisplayName = displayName;
            user.DisplayNameNormalized = displayName.ToUpperInvariant();
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Counts non-overlapping occurrences of <paramref name="target"/> in <paramref name="source"/>.
        /// </summary>
        private static int CountOccurrences(string source, string target)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(target, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += target.Length;
            }
            return count;
        }

        // -------------------------------------------------------------------------
        // GET /Leaderboards — TotalBond
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetLeaderboards_TotalBond_Returns200()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 1);

            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=0&page=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLeaderboards_TotalBond_RendersUserDisplayNamesInDescendingBondOrder()
        {
            // Arrange — 3 users with known bond sums, seeded so ranking is deterministic
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 1);

            var user1 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user2 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user3 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await SetDisplayNameAsync(context, user1.Id, "Topaz");
            await SetDisplayNameAsync(context, user2.Id, "Silver");
            await SetDisplayNameAsync(context, user3.Id, "Bronze");

            // Topaz → highest bond (rank 1), Silver → middle (rank 2), Bronze → lowest (rank 3)
            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1, bond: 100);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 1, bond: 50);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user3.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user1.Id, user1.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=0&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — all names present and in correct order
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var topazIdx = html.IndexOf("Topaz", StringComparison.Ordinal);
            var silverIdx = html.IndexOf("Silver", StringComparison.Ordinal);
            var bronzeIdx = html.IndexOf("Bronze", StringComparison.Ordinal);
            Assert.True(topazIdx >= 0, "Topaz should appear in the HTML");
            Assert.True(silverIdx >= 0, "Silver should appear in the HTML");
            Assert.True(bronzeIdx >= 0, "Bronze should appear in the HTML");
            Assert.True(topazIdx < silverIdx, "Topaz should appear before Silver");
            Assert.True(silverIdx < bronzeIdx, "Silver should appear before Bronze");
        }

        [Fact]
        public async Task GetLeaderboards_TotalBond_RendersCorrectDenseRankBadges_WhenTopTwoAreTied()
        {
            // Arrange — users A and B tie at top (both rank 1); user C is rank 2
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 1);

            var user1 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user2 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user3 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1, bond: 50);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 1, bond: 50);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user3.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user1.Id, user1.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=0&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — "#1" badge appears twice (tied pair), "#2" once (dense), "#3" not present
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, CountOccurrences(html, "#1"));
            Assert.Equal(1, CountOccurrences(html, "#2"));
            Assert.DoesNotContain("#3", html);
        }

        [Fact]
        public async Task GetLeaderboards_TotalBond_ShowsPaginationControls_WhenResultsExceedPageSize()
        {
            // Arrange — seed one more user than the page size
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await IntegrationTestHelpers.SeedGirlsAsync(context, 1);

            var viewer = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            for (int i = 0; i <= GameConstants.LeaderboardPageSize; i++)
            {
                var u = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
                await IntegrationTestHelpers.AdoptGirlAsync(context, u.Id, 1, bond: 100 - i);
            }

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, viewer.Id, viewer.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=0&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — pagination container and Next button are rendered
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("friends-pagination", html);
            Assert.Contains("eg-nav-next", html);
        }

        // -------------------------------------------------------------------------
        // GET /Leaderboards — CompanionBond
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetLeaderboards_CompanionBond_Returns200WithCorrectOrderAndRanks()
        {
            // Arrange — one companion, 3 users with known individual bonds
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await SeedGirlAsync(context, 1, "Aoi", "/images/aoi.jpg");

            var user1 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user2 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user3 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await SetDisplayNameAsync(context, user1.Id, "Plum");
            await SetDisplayNameAsync(context, user2.Id, "Maple");
            await SetDisplayNameAsync(context, user3.Id, "Cedar");

            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1, bond: 80);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 1, bond: 40);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user3.Id, 1, bond: 5);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user1.Id, user1.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=1&girlId=1&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — 200 OK, correct ordering, and correct rank badges
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var plumIdx = html.IndexOf("Plum", StringComparison.Ordinal);
            var mapleIdx = html.IndexOf("Maple", StringComparison.Ordinal);
            var cedarIdx = html.IndexOf("Cedar", StringComparison.Ordinal);
            Assert.True(plumIdx >= 0, "Plum should appear in the HTML");
            Assert.True(plumIdx < mapleIdx, "Plum (highest bond) should appear before Maple");
            Assert.True(mapleIdx < cedarIdx, "Maple should appear before Cedar");

            // Distinct bonds → ranks 1, 2, 3
            Assert.Contains("#1", html);
            Assert.Contains("#2", html);
            Assert.Contains("#3", html);
        }

        [Fact]
        public async Task GetLeaderboards_CompanionBond_ShowsCompanionNameInHeroTitle()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await SeedGirlAsync(context, 1, "Sakura", "/images/sakura.jpg");

            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=1&girlId=1&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — hero title produced by the controller: "Bond with {name}"
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Bond with Sakura", html);
        }

        [Fact]
        public async Task GetLeaderboards_CompanionBond_RendersPortraitImage_WhenImageUrlExists()
        {
            // Arrange — companion with a valid ImageUrl
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await SeedGirlAsync(context, 1, "Hana", "/images/hana.jpg");

            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=1&girlId=1&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — <img> portrait is rendered, letter circle is NOT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("leaderboard-companion-hero-portrait", html);
            Assert.Contains("/images/hana.jpg", html);
            Assert.DoesNotContain("leaderboard-companion-hero-portrait--letter", html);
        }

        [Fact]
        public async Task GetLeaderboards_CompanionBond_RendersLetterCircle_WhenImageUrlIsNull()
        {
            // Arrange — companion without an ImageUrl
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await SeedGirlAsync(context, 1, "Nami", string.Empty); // empty = no image; view IsNullOrEmpty check triggers letter circle

            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=1&girlId=1&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — letter circle fallback is rendered with the first letter of the companion name
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("leaderboard-companion-hero-portrait--letter", html);
            Assert.Contains(">N<", html); // first letter of "Nami" inside the <span>
        }

        [Fact]
        public async Task GetLeaderboards_CompanionBond_TiedUsers_ShareSameRankBadge()
        {
            // Arrange — two users tied on companion bond (both rank 1), one below (rank 2)
            using var scope = _factory.Services.CreateScope();
            var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
            await SeedGirlAsync(context, 1, "Yuki", "/images/yuki.jpg");

            var user1 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user2 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);
            var user3 = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            await IntegrationTestHelpers.AdoptGirlAsync(context, user1.Id, 1, bond: 60);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user2.Id, 1, bond: 60);
            await IntegrationTestHelpers.AdoptGirlAsync(context, user3.Id, 1, bond: 10);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user1.Id, user1.Email!);

            // Act
            var response = await client.GetAsync("/Leaderboards?type=1&girlId=1&page=1");
            var html = await response.Content.ReadAsStringAsync();

            // Assert — "#1" appears twice (tied), "#2" once (dense rank)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, CountOccurrences(html, "#1"));
            Assert.Equal(1, CountOccurrences(html, "#2"));
            Assert.DoesNotContain("#3", html);
        }

        // -------------------------------------------------------------------------
        // POST — Not Allowed
        // -------------------------------------------------------------------------

        [Fact]
        public async Task PostLeaderboards_IsNotAllowed()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var user = await IntegrationTestHelpers.CreateTestUserAsync(scope.ServiceProvider);

            var client = _factory.CreateClientNoRedirect();
            IntegrationTestHelpers.AuthenticateClient(client, user.Id, user.Email!);

            // Act
            using var content = new FormUrlEncodedContent([]);
            var response = await client.PostAsync("/Leaderboards", content);

            // Assert — POST to an [HttpGet]-only action must be rejected
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }
    }
}
