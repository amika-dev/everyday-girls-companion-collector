using System;
using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EverydayGirlsCompanionCollector
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Check if using SQLite (via configuration for testability)
            var useSqlite = builder.Configuration.GetValue<bool>("Testing:UseSqlite");
            
            if (useSqlite)
            {
                // SQLite for integration tests
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            }
            else
            {
                // SQL Server for production with transient fault retry
                var connString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connString, sql => 
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)));
            }

            // Add ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Simple password requirements for MVP
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Stamp DisplayName into the auth cookie so it is available without a DB query
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsFactory>();

            // Configure cookie authentication
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            // Configure forwarded headers for Azure App Service / reverse proxy
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Azure App Service and most reverse proxies use these headers
                // Clear known networks/proxies to trust all (safe for App Service)
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Register application services
            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddSingleton<IRandom, SystemRandom>();
            builder.Services.AddScoped<IDailyStateService, DailyStateService>();
            builder.Services.AddSingleton<IDialogueService, DialogueService>();
            builder.Services.AddScoped<IDailyRollService, DailyRollService>();
            builder.Services.AddScoped<IAdoptionService, AdoptionService>();
            builder.Services.AddSingleton<IGameplayTipService, GameplayTipService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IFriendsQuery, FriendsQuery>();
            builder.Services.AddScoped<IFriendsService, FriendsService>();
            builder.Services.AddScoped<IFriendProfileQuery, FriendProfileQuery>();
            builder.Services.AddScoped<IFriendCollectionQuery, FriendCollectionQuery>();
            builder.Services.AddScoped<ILeaderboardQuery, LeaderboardQuery>();

            // Add MVC services
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Apply database migrations and seed if enabled (non-fatal)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    // Apply migrations automatically on startup
                    logger.LogInformation("Applying migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Migrations applied successfully.");

                    // Seed the database if enabled
                    if (app.Configuration.GetValue<bool>("Seeding:Enable"))
                    {
                        DbInitializer.Initialize(context);
                        logger.LogInformation("Database seeding completed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database migration/seed failed at startup.");
                    // Do not rethrow - allow app to start even if DB is unavailable
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Must remain before UseHttpsRedirection to properly handle X-Forwarded-Proto
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseRouting();

            // Authentication must come before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

