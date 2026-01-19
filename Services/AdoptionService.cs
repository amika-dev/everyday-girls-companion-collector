namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Implementation of adoption service.
    /// </summary>
    public class AdoptionService : IAdoptionService
    {
        /// <inheritdoc />
        public bool CanAdopt(int currentCollectionSize, int maxSize)
        {
            if (currentCollectionSize < 0)
            {
                throw new ArgumentException("Collection size cannot be negative.", nameof(currentCollectionSize));
            }

            if (maxSize <= 0)
            {
                throw new ArgumentException("Max size must be positive.", nameof(maxSize));
            }

            return currentCollectionSize < maxSize;
        }

        /// <inheritdoc />
        public bool ShouldSetAsPartner(int? currentPartnerId)
        {
            return currentPartnerId == null;
        }
    }
}
