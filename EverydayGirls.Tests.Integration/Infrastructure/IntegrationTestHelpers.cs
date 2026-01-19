using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace EverydayGirls.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Helper utilities for integration tests.
    /// </summary>
    /// <remarks>
    /// Provides methods for:
    /// - Creating and authenticating test users
    /// - Seeding girl data
    /// - Setting up user state (collection, partner, daily state)
    /// - Accessing database context within test scopes
    /// </remarks>
    public static class IntegrationTestHelpers
    {
        /// <summary>
        /// Seeds a minimal set of girls into the database for testing.
        /// </summary>
        public static async Task SeedGirlsAsync(ApplicationDbContext context, int count = 10)
        {
            if (context.Girls.Any())
            {
                return; // Already seeded
            }

            var girls = new List<Girl>();
            for (int i = 1; i <= count; i++)
            {
                girls.Add(new Girl
                {
                    Name = $"TestGirl{i:D3}",
                    ImageUrl = $"/images/girls/{i:D3}.jpg"
                });
            }

            context.Girls.AddRange(girls);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a test user with Identity and initializes their daily state.
        /// </summary>
        /// <param name="serviceProvider">Service provider to resolve UserManager and DbContext.</param>
        /// <param name="email">User email address (if null, generates unique email).</param>
        /// <param name="password">User password (default: "Test123!").</param>
        /// <returns>The created ApplicationUser with UserId populated.</returns>
        public static async Task<ApplicationUser> CreateTestUserAsync(
            IServiceProvider serviceProvider,
            string? email = null,
            string password = "Test123!")
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Generate unique email if not provided
            if (string.IsNullOrEmpty(email))
            {
                email = $"testuser-{Guid.NewGuid()}@example.com";
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Initialize daily state
            var dailyState = new UserDailyState
            {
                UserId = user.Id
            };
            context.UserDailyStates.Add(dailyState);
            await context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Adopts a girl for the specified user.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="girlId">Girl ID to adopt.</param>
        /// <param name="dateMetUtc">Adoption date (optional, defaults to 2026-01-15 00:00 UTC).</param>
        /// <param name="bond">Initial bond level (default: 0).</param>
        /// <param name="personality">Personality tag (default: Cheerful).</param>
        public static async Task AdoptGirlAsync(
            ApplicationDbContext context,
            string userId,
            int girlId,
            DateTime? dateMetUtc = null,
            int bond = 0,
            PersonalityTag personality = PersonalityTag.Cheerful)
        {
            var userGirl = new UserGirl
            {
                UserId = userId,
                GirlId = girlId,
                DateMetUtc = dateMetUtc ?? new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Bond = bond,
                PersonalityTag = personality
            };

            context.UserGirls.Add(userGirl);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Sets the user's partner.
        /// </summary>
        public static async Task SetPartnerAsync(
            ApplicationDbContext context,
            string userId,
            int girlId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not found");
            }

            user.PartnerGirlId = girlId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the user's daily state.
        /// </summary>
        public static async Task UpdateDailyStateAsync(
            ApplicationDbContext context,
            string userId,
            DateOnly? lastRollDate = null,
            DateOnly? lastAdoptDate = null,
            DateOnly? lastInteractionDate = null,
            int[] candidateIds = null!)
        {
            var state = await context.UserDailyStates.FindAsync(userId);
            if (state == null)
            {
                state = new UserDailyState { UserId = userId };
                context.UserDailyStates.Add(state);
            }

            if (lastRollDate.HasValue)
            {
                state.LastDailyRollDate = lastRollDate;
            }

            if (lastAdoptDate.HasValue)
            {
                state.LastDailyAdoptDate = lastAdoptDate;
            }

            if (lastInteractionDate.HasValue)
            {
                state.LastDailyInteractionDate = lastInteractionDate;
            }

            if (candidateIds != null && candidateIds.Length > 0)
            {
                state.CandidateDate = lastRollDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                state.Candidate1GirlId = candidateIds.Length > 0 ? candidateIds[0] : null;
                state.Candidate2GirlId = candidateIds.Length > 1 ? candidateIds[1] : null;
                state.Candidate3GirlId = candidateIds.Length > 2 ? candidateIds[2] : null;
                state.Candidate4GirlId = candidateIds.Length > 3 ? candidateIds[3] : null;
                state.Candidate5GirlId = candidateIds.Length > 4 ? candidateIds[4] : null;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a scoped database context for test queries.
        /// </summary>
        /// <remarks>
        /// Usage:
        /// <code>
        /// using var scope = factory.Services.CreateScope();
        /// var context = IntegrationTestHelpers.GetDbContext(scope.ServiceProvider);
        /// var users = await context.Users.ToListAsync();
        /// </code>
        /// </remarks>
        public static ApplicationDbContext GetDbContext(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Authenticates an HttpClient as a test user by setting authentication headers.
        /// These headers are read by TestAuthHandler to create an authenticated ClaimsPrincipal.
        /// </summary>
        /// <param name="client">HttpClient to authenticate.</param>
        /// <param name="userId">User ID to authenticate as.</param>
        /// <param name="email">Email address of the user.</param>
        public static void AuthenticateClient(HttpClient client, string userId, string email)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId);
            client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        }
    }
}



