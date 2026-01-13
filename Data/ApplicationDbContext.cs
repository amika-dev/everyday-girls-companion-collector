using EverydayGirlsCompanionCollector.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Data
{
    /// <summary>
    /// Application database context extending Identity with custom entities.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Global pool of girls available for adoption.
        /// </summary>
        public DbSet<Girl> Girls { get; set; } = null!;

        /// <summary>
        /// User-owned girls with bond and personality data.
        /// </summary>
        public DbSet<UserGirl> UserGirls { get; set; } = null!;

        /// <summary>
        /// Daily state tracking for each user.
        /// </summary>
        public DbSet<UserDailyState> UserDailyStates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Girl entity
            modelBuilder.Entity<Girl>(entity =>
            {
                entity.HasKey(g => g.GirlId);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.ImageUrl).IsRequired().HasMaxLength(500);
            });

            // Configure UserGirl with composite key
            modelBuilder.Entity<UserGirl>(entity =>
            {
                // Composite primary key
                entity.HasKey(ug => new { ug.UserId, ug.GirlId });

                // Relationships
                entity.HasOne(ug => ug.User)
                    .WithMany()
                    .HasForeignKey(ug => ug.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Girl)
                    .WithMany()
                    .HasForeignKey(ug => ug.GirlId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for sorting (as per Implementation Checklist)
                entity.HasIndex(ug => new { ug.UserId, ug.Bond, ug.DateMetUtc })
                    .HasDatabaseName("IX_UserGirls_UserId_Bond_DateMet")
                    .IsDescending(false, true, false); // UserId ASC, Bond DESC, DateMet ASC

                entity.HasIndex(ug => new { ug.UserId, ug.DateMetUtc })
                    .HasDatabaseName("IX_UserGirls_UserId_DateMet_Asc");

                entity.HasIndex(ug => new { ug.UserId, ug.DateMetUtc })
                    .HasDatabaseName("IX_UserGirls_UserId_DateMet_Desc")
                    .IsDescending(false, true); // UserId ASC, DateMet DESC
            });

            // Configure UserDailyState
            modelBuilder.Entity<UserDailyState>(entity =>
            {
                entity.HasKey(uds => uds.UserId);

                entity.HasOne(uds => uds.User)
                    .WithOne()
                    .HasForeignKey<UserDailyState>(uds => uds.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationUser partner relationship
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Partner)
                    .WithMany()
                    .HasForeignKey(u => u.PartnerGirlId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
