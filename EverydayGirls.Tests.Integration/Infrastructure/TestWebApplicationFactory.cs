using EverydayGirls.Tests.Integration.TestDoubles;
using EverydayGirlsCompanionCollector;
using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EverydayGirls.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Custom WebApplicationFactory for integration testing.
    /// Replaces SQL Server with SQLite in-memory and injects test doubles for time/randomness.
    /// </summary>
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection? _connection;

        /// <summary>
        /// Gets the test clock instance used by the application.
        /// </summary>
        public TestClock TestClock { get; }

        /// <summary>
        /// Gets the test random instance used by the application.
        /// </summary>
        public TestRandom TestRandom { get; }

        public TestWebApplicationFactory(string? databaseName = null)
        {
            // Initialize test doubles
            TestClock = new TestClock(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));
            TestRandom = new TestRandom(Enumerable.Range(0, 100).ToArray());

            // Create and open SQLite in-memory connection
            // Using unique name per factory instance to isolate tests
            var dbName = databaseName ?? $"TestDb-{Guid.NewGuid()}";
            _connection = new SqliteConnection($"Data Source={dbName};Mode=Memory;Cache=Shared");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Signal to Program.cs to use SQLite via configuration
            // Required because Program.cs conditionally registers DbContext before ConfigureWebHost runs
            builder.UseSetting("Testing:UseSqlite", "true");
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connection!.ConnectionString);

            builder.ConfigureServices(services =>
            {
                // Remove production DbContext registrations to prevent SQL Server/SQLite conflicts
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();

                // Re-add DbContext with SQLite in-memory using the shared open connection
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));

                // Replace authentication with test authentication handler
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    options => { });

                // Replace IAntiforgery with test double that always validates
                services.RemoveAll<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
                services.AddSingleton<Microsoft.AspNetCore.Antiforgery.IAntiforgery, TestDoubles.TestAntiforgery>();

                // Replace IClock with TestClock for deterministic time-based testing
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(TestClock);

                // Replace IRandom with TestRandom for deterministic randomness
                services.RemoveAll<IRandom>();
                services.AddSingleton<IRandom>(TestRandom);
            });

            // Use test environment
            builder.UseEnvironment("Test");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Build the host with services configured in ConfigureWebHost
            var host = base.CreateHost(builder);

            // Ensure the SQLite database schema is created once the host is built
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            }

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Close and dispose the SQLite connection
                _connection?.Close();
                _connection?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates an HttpClient configured for integration tests.
        /// By default, does NOT follow redirects so tests can assert redirect responses.
        /// </summary>
        public HttpClient CreateClientNoRedirect()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
    }
}






