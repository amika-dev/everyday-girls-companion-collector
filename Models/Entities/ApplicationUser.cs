using Microsoft.AspNetCore.Identity;

namespace EverydayGirlsCompanionCollector.Models.Entities
{
    /// <summary>
    /// Application user extending ASP.NET Core Identity with partner tracking.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Foreign key to the current partner girl. Nullable - first adoption sets this automatically.
        /// </summary>
        public int? PartnerGirlId { get; set; }

        /// <summary>
        /// Navigation property to the partner girl.
        /// </summary>
        public Girl? Partner { get; set; }
    }
}
