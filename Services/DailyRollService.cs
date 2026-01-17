using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Models.Entities;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Implementation of daily roll service.
    /// </summary>
    public class DailyRollService : IDailyRollService
    {
        private readonly IRandom _random;

        public DailyRollService(IRandom random)
        {
            _random = random;
        }

        /// <inheritdoc />
        public List<Girl> GenerateCandidates(Girl[] availableGirls, int count)
        {
            ArgumentNullException.ThrowIfNull(availableGirls);

            if (count <= 0)
            {
                return new List<Girl>();
            }

            _random.Shuffle(availableGirls);
            return availableGirls.Take(count).ToList();
        }
    }
}
