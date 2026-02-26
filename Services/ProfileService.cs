using System.Text.RegularExpressions;
using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Implementation of <see cref="IProfileService"/>.
    /// Reads profile summaries and enforces display name change rules.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClock _clock;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Mirrors the DB CHECK constraint: 4–16 alphanumeric characters.
        // See DatabaseConstraints.DisplayNameCheckConstraintSql and GameConstants for limits.
        private static readonly Regex DisplayNamePattern = new(
            $"^[a-zA-Z0-9]{{{GameConstants.DisplayNameMinLength},{GameConstants.DisplayNameMaxLength}}}$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

        public ProfileService(ApplicationDbContext context, IClock clock, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _clock = clock;
            _signInManager = signInManager;
        }

        /// <inheritdoc />
        public async Task<ProfileViewModel> GetProfileAsync(string userId, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var user = await _context.Users
                .Include(u => u.Partner)
                .FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new InvalidOperationException($"We couldn't find a profile for user '{userId}'.");

            // Single query to get all collection data needed for totals and partner bond.
            var userGirls = await _context.UserGirls
                .Where(ug => ug.UserId == userId)
                .ToListAsync(ct);

            var totalBond = userGirls.Sum(ug => ug.Bond);
            var totalCompanions = userGirls.Count;

            var currentServerDate = DailyCadence.GetServerDateFromUtc(_clock.UtcNow);
            var partnerData = user.PartnerGirlId.HasValue
                ? userGirls.FirstOrDefault(ug => ug.GirlId == user.PartnerGirlId.Value)
                : null;

            return new ProfileViewModel
            {
                DisplayName = user.DisplayName,
                PartnerName = user.Partner?.Name,
                PartnerImageUrl = user.Partner?.ImageUrl,
                PartnerBond = partnerData?.Bond,
                PartnerFirstMetUtc = partnerData?.DateMetUtc,
                PartnerDaysTogether = partnerData is not null
                    ? DailyCadence.GetDaysSinceAdoption(currentServerDate, partnerData.DateMetUtc)
                    : null,
                PartnerPersonalityTag = partnerData?.PersonalityTag,
                TotalBond = totalBond,
                TotalCompanions = totalCompanions,
                CanChangeDisplayName = CanChangeDisplayName(user.LastDisplayNameChangeUtc, currentServerDate)
            };
        }

        /// <inheritdoc />
        public async Task<DisplayNameChangeResult> TryChangeDisplayNameAsync(
            string userId, string newDisplayName, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            // 1. Format: 4–16 alphanumeric characters.
            // newDisplayName comes from user input; treat empty/whitespace as a format failure,
            // not an ArgumentException, so the caller receives a result rather than an exception.
            if (string.IsNullOrEmpty(newDisplayName) || !DisplayNamePattern.IsMatch(newDisplayName))
            {
                return DisplayNameChangeResult.Failure(
                    $"We're sorry, Display names need to be {GameConstants.DisplayNameMinLength}–{GameConstants.DisplayNameMaxLength} letters or numbers — no spaces or special characters allowed.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, ct)
                ?? throw new InvalidOperationException($"We couldn't find a profile for user '{userId}'.");

            var normalized = newDisplayName.ToUpperInvariant();

            // 2. Must differ from the current name (case-insensitive).
            if (normalized == user.DisplayNameNormalized)
            {
                return DisplayNameChangeResult.Failure("That's already your name. Nothing to change here!");
            }

            // 3. Once per ServerDate.
            var currentServerDate = DailyCadence.GetServerDateFromUtc(_clock.UtcNow);
            if (!CanChangeDisplayName(user.LastDisplayNameChangeUtc, currentServerDate))
            {
                return DisplayNameChangeResult.Failure("You've already chosen a new name today. Come back after the next reset \u2728");
            }

            // 4. Case-insensitive uniqueness across all users.
            var isTaken = await _context.Users
                .AnyAsync(u => u.Id != userId && u.DisplayNameNormalized == normalized, ct);

            if (isTaken)
            {
                return DisplayNameChangeResult.Failure("Hmm, that name is already taken. Would you like to try something a little different?");
            }

            // 5. Apply the change.
            user.DisplayName = newDisplayName;
            user.DisplayNameNormalized = normalized;
            user.LastDisplayNameChangeUtc = _clock.UtcNow;

            await _context.SaveChangesAsync(ct);

            // Re-issue the auth cookie with the updated DisplayName claim so the
            // navbar reflects the change immediately without requiring a logout.
            await _signInManager.RefreshSignInAsync(user);

            return DisplayNameChangeResult.Success();
        }

        /// <summary>
        /// Returns true if the user has not yet changed their display name during the current ServerDate.
        /// </summary>
        private static bool CanChangeDisplayName(DateTime? lastChangedUtc, DateOnly currentServerDate)
        {
            if (lastChangedUtc == null)
            {
                return true;
            }

            var lastChangeServerDate = DailyCadence.GetServerDateFromUtc(lastChangedUtc.Value);
            return lastChangeServerDate != currentServerDate;
        }
    }
}
