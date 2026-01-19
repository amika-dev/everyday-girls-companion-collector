using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
                // SQL Server for production
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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

            // Configure cookie authentication
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            // Register application services
            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddSingleton<IRandom, SystemRandom>();
            builder.Services.AddScoped<IDailyStateService, DailyStateService>();
            builder.Services.AddSingleton<IDialogueService, DialogueService>();
            builder.Services.AddScoped<IDailyRollService, DailyRollService>();
            builder.Services.AddScoped<IAdoptionService, AdoptionService>();
            builder.Services.AddSingleton<IGameplayTipService, GameplayTipService>();

            // Add MVC services
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Seed the database
            if (app.Configuration.GetValue<bool>("Seeding:Enable"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;

                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        DbInitializer.Initialize(context);
                    }
                    catch (Exception ex)
                    {
                        var logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

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

