namespace EverydayGirlsCompanionCollector.Models
{
    /// <summary>
    /// A page of items with total count metadata. Page numbering is 1-based.
    /// </summary>
    /// <typeparam name="T">The type of items in the page.</typeparam>
    public record PagedResult<T>
    {
        /// <summary>
        /// The items on the current page.
        /// </summary>
        public required IReadOnlyList<T> Items { get; init; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public required int Page { get; init; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public required int PageSize { get; init; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        /// <summary>
        /// True if there is a previous page.
        /// </summary>
        public bool HasPrevious => Page > 1;

        /// <summary>
        /// True if there is a next page.
        /// </summary>
        public bool HasNext => Page < TotalPages;

        /// <summary>
        /// Creates an empty result for the given page parameters.
        /// </summary>
        public static PagedResult<T> Empty(int page, int pageSize) => new()
        {
            Items = [],
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        /// <summary>
        /// Clamps page and pageSize to valid ranges. Page &lt; 1 becomes 1; pageSize &lt;= 0 becomes <paramref name="defaultPageSize"/>.
        /// </summary>
        public static (int Page, int PageSize) Clamp(int page, int pageSize, int defaultPageSize)
        {
            var clampedPage = page < 1 ? 1 : page;
            var clampedPageSize = pageSize <= 0 ? defaultPageSize : pageSize;
            return (clampedPage, clampedPageSize);
        }
    }
}
