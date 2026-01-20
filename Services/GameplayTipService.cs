using EverydayGirlsCompanionCollector.Abstractions;
using EverydayGirlsCompanionCollector.Constants;
using EverydayGirlsCompanionCollector.Models;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Provides gameplay tips and hints to users.
    /// </summary>
    public class GameplayTipService : IGameplayTipService
    {
        private readonly IRandom _random;
        
        private static readonly IReadOnlyList<GameplayTip> _tips = new List<GameplayTip>
        {
            new GameplayTip
            {
                Id = "bond",
                Title = "Building Bonds",
                Body = "Spending time together increases Bond.",
                Icon = "♥"
            },
            new GameplayTip
            {
                Id = "collection-size",
                Title = "Collection Limit",
                Body = $"You can keep up to {GameConstants.MaxCollectionSize} companions in your collection.",
                Icon = "👥"
            },
            new GameplayTip
            {
                Id = "daily-reset",
                Title = "Daily Reset",
                Body = $"Most actions refresh each day at {GameConstants.DailyResetHourUtc}:00 UTC.",
                Icon = "🕐"
            },
            new GameplayTip
            {
                Id = "roll-persistence",
                Title = "Today's Candidates",
                Body = "Your daily candidates stay the same until the next reset.",
                Icon = "🎲"
            },
            new GameplayTip
            {
                Id = "partner-change",
                Title = "Changing Partners",
                Body = "You can change your partner anytime from your collection.",
                Icon = "✨"
            },
            new GameplayTip
            {
                Id = "first-adoption",
                Title = "Your First Companion",
                Body = "Your first adoption automatically becomes your partner.",
                Icon = "🌸"
            },
            new GameplayTip
            {
                Id = "personality-tags-effect",
                Title = "Personalities Matter",
                Body = "Each partner's personality tag affects the dialogue you'll hear during interactions.",
                Icon = "💬"
            },
            new GameplayTip
            {
                Id = "personality-tags-change",
                Title = "Change of Mood",
                Body = "You can change your companions' personality tags anytime in your collection.",
                Icon = "💭"
            }
        }.AsReadOnly();

        public GameplayTipService(IRandom random)
        {
            ArgumentNullException.ThrowIfNull(random);
            _random = random;
        }

        public IReadOnlyList<GameplayTip> GetAllTips()
        {
            return _tips;
        }

        public GameplayTip GetRandomTip()
        {
            if (_tips.Count == 0)
            {
                throw new InvalidOperationException("No tips available.");
            }

            var index = _random.Next(_tips.Count);
            return _tips[index];
        }
    }
}
