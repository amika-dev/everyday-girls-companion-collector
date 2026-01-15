using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Models.ViewModels
{
    /// <summary>
    /// View model for the Collection screen with pagination and sorting.
    /// </summary>
    public class CollectionViewModel
    {
        /// <summary>
        /// List of owned girls for current page.
        /// </summary>
        public List<CollectionGirlViewModel> Girls { get; set; } = new();

        /// <summary>
        /// Current sort mode.
        /// </summary>
        public string SortMode { get; set; } = "bond";

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Current partner's girl ID (for highlighting).
        /// </summary>
        public int? PartnerGirlId { get; set; }
    }

    /// <summary>
    /// Represents a single girl in the collection grid.
    /// </summary>
    public class CollectionGirlViewModel
    {
        public int GirlId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Bond { get; set; }
        public DateTime DateMetUtc { get; set; }
        public PersonalityTag PersonalityTag { get; set; }
        public bool IsPartner { get; set; }

        /// <summary>
        /// Calculated number of days since adoption (Days Together).
        /// </summary>
        public int DaysSinceAdoption => (int)(DateTime.UtcNow - DateMetUtc).TotalDays;
    }
}
