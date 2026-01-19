namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service for managing adoption rules and validation.
    /// </summary>
    public interface IAdoptionService
    {
        /// <summary>
        /// Checks if user can adopt another girl based on collection size limit.
        /// </summary>
        /// <param name="currentCollectionSize">Current number of owned girls.</param>
        /// <param name="maxSize">Maximum collection size.</param>
        /// <returns>True if adoption is allowed.</returns>
        bool CanAdopt(int currentCollectionSize, int maxSize);

        /// <summary>
        /// Determines if this adoption should set the girl as partner (first adoption).
        /// </summary>
        /// <param name="currentPartnerId">Current partner ID (null if no partner).</param>
        /// <returns>True if this should be set as partner.</returns>
        bool ShouldSetAsPartner(int? currentPartnerId);
    }
}
