using EverydayGirlsCompanionCollector.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Stamps the user's DisplayName into the auth cookie as a claim at sign-in,
    /// making it available via User.FindFirst("DisplayName") throughout the app
    /// without a per-request database query.
    /// </summary>
    internal sealed class ApplicationUserClaimsFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : UserClaimsPrincipalFactory<ApplicationUser>(userManager, optionsAccessor)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("DisplayName", user.DisplayName));
            return identity;
        }
    }
}
