using EverydayGirlsCompanionCollector.Constants;
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
        /// User-owned girls with bond, personality, and skill data.
        /// </summary>
        public DbSet<UserGirl> UserGirls { get; set; } = null!;

        /// <summary>
        /// Daily state tracking for each user.
        /// </summary>
        public DbSet<UserDailyState> UserDailyStates { get; set; } = null!;

        /// <summary>
        /// Town locations where companions can be assigned.
        /// </summary>
        public DbSet<TownLocation> TownLocations { get; set; } = null!;

        /// <summary>
        /// Friend relationships between users.
        /// </summary>
        public DbSet<FriendRelationship> FriendRelationships { get; set; } = null!;

        /// <summary>
        /// Companion assignments to town locations.
        /// </summary>
        public DbSet<CompanionAssignment> CompanionAssignments { get; set; } = null!;

        /// <summary>
        /// User unlocks for locked town locations.
        /// </summary>
        public DbSet<UserTownLocationUnlock> UserTownLocationUnlocks { get; set; } = null!;

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

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Partner relationship
                entity.HasOne(u => u.Partner)
                    .WithMany()
                    .HasForeignKey(u => u.PartnerGirlId)
                    .OnDelete(DeleteBehavior.Restrict);

                // DisplayName fields
                entity.Property(u => u.DisplayName)
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasDefaultValue("Townie");

                entity.Property(u => u.DisplayNameNormalized)
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasDefaultValue("TOWNIE");

                entity.Property(u => u.CurrencyBalance)
                    .IsRequired()
                    .HasDefaultValue(0);

                // Non-unique index on normalized display name for case-insensitive lookups
                entity.HasIndex(u => u.DisplayNameNormalized)
                    .HasDatabaseName("IX_AspNetUsers_DisplayNameNormalized");

                // CHECK constraint: DisplayName length 4-16, alphanumeric only (SQL Server only)
                // Note: SQLite doesn't support LEN() or LIKE with character class patterns
                if (Database.IsSqlServer())
                {
                    entity.ToTable(tb => tb.HasCheckConstraint(
                        "CK_AspNetUsers_DisplayName_Valid",
                        DatabaseConstraints.DisplayNameCheckConstraintSql));
                }
            });

            // Configure TownLocation
            modelBuilder.Entity<TownLocation>(entity =>
            {
                entity.HasKey(tl => tl.TownLocationId);

                entity.Property(tl => tl.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(tl => tl.PrimarySkill)
                    .IsRequired();

                entity.Property(tl => tl.BaseDailyBondGain)
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(tl => tl.BaseDailyCurrencyGain)
                    .IsRequired()
                    .HasDefaultValue(5);

                entity.Property(tl => tl.BaseDailySkillGain)
                    .IsRequired()
                    .HasDefaultValue(10);

                entity.Property(tl => tl.IsLockedByDefault)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(tl => tl.UnlockCost)
                    .IsRequired()
                    .HasDefaultValue(50);
            });

            // Configure FriendRelationship
            modelBuilder.Entity<FriendRelationship>(entity =>
            {
                // Composite primary key
                entity.HasKey(fr => new { fr.UserId, fr.FriendUserId });

                // Relationships - use Restrict to avoid cascade cycles
                entity.HasOne(fr => fr.User)
                    .WithMany(u => u.Friends)
                    .HasForeignKey(fr => fr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(fr => fr.Friend)
                    .WithMany(u => u.FriendOf)
                    .HasForeignKey(fr => fr.FriendUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index on FriendUserId for reverse lookups
                entity.HasIndex(fr => fr.FriendUserId)
                    .HasDatabaseName("IX_FriendRelationships_FriendUserId");

                entity.Property(fr => fr.DateAddedUtc)
                    .IsRequired();
            });

            // Configure CompanionAssignment
            modelBuilder.Entity<CompanionAssignment>(entity =>
            {
                // Composite primary key (same as UserGirl)
                entity.HasKey(ca => new { ca.UserId, ca.GirlId });

                // Composite FK to UserGirl
                entity.HasOne(ca => ca.UserGirl)
                    .WithOne()
                    .HasForeignKey<CompanionAssignment>(ca => new { ca.UserId, ca.GirlId })
                    .OnDelete(DeleteBehavior.Cascade);

                // FK to TownLocation
                entity.HasOne(ca => ca.TownLocation)
                    .WithMany()
                    .HasForeignKey(ca => ca.TownLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index on TownLocationId
                entity.HasIndex(ca => ca.TownLocationId)
                    .HasDatabaseName("IX_CompanionAssignments_TownLocationId");

                entity.Property(ca => ca.AssignedUtc)
                    .IsRequired();
            });

            // Configure UserTownLocationUnlock
            modelBuilder.Entity<UserTownLocationUnlock>(entity =>
            {
                // Composite primary key
                entity.HasKey(utlu => new { utlu.UserId, utlu.TownLocationId });

                // Relationships
                entity.HasOne(utlu => utlu.User)
                    .WithMany()
                    .HasForeignKey(utlu => utlu.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(utlu => utlu.TownLocation)
                    .WithMany()
                    .HasForeignKey(utlu => utlu.TownLocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(utlu => utlu.UnlockedUtc)
                    .IsRequired();
            });
        }
    }
}
