using EverydayGirlsCompanionCollector.Models.Enums;

namespace EverydayGirlsCompanionCollector.Services
{
    /// <summary>
    /// Service providing random dialogue lines based on personality tags.
    /// </summary>
    public class DialogueService : IDialogueService
    {
        private readonly Dictionary<PersonalityTag, List<string>> _dialoguePool = new()
        {
            [PersonalityTag.Cheerful] = new List<string>
            {
                "Today is such a wonderful day!",
                "I'm so happy to see you!",
                "Let's make today amazing!",
                "Your smile always brightens my day!",
                "Everything feels so much better when you're around!",
                "I love spending time with you!",
                "You always know how to make me laugh!",
                "This is the best part of my day!",
                "I'm so grateful we met!",
                "Life is so much more fun with you!"
            },
            [PersonalityTag.Shy] = new List<string>
            {
                "Um... hi...",
                "I... I'm glad you're here...",
                "Thanks for spending time with me...",
                "I feel safe when you're around...",
                "You're really kind...",
                "I hope I'm not bothering you...",
                "This is... nice...",
                "I've been thinking about you...",
                "You make me feel comfortable...",
                "I'm happy we can be together like this..."
            },
            [PersonalityTag.Energetic] = new List<string>
            {
                "Let's go! Let's go!",
                "I've got so much energy today!",
                "What should we do next?!",
                "This is so exciting!",
                "I can't sit still when I'm with you!",
                "Come on, let's have some fun!",
                "I'm ready for anything!",
                "You're keeping up pretty well!",
                "There's never a dull moment with you!",
                "Let's make this the best day ever!"
            },
            [PersonalityTag.Calm] = new List<string>
            {
                "It's peaceful here with you.",
                "I enjoy these quiet moments.",
                "Take your time, there's no rush.",
                "Everything feels so serene.",
                "I appreciate your presence.",
                "Let's just relax together.",
                "This moment is enough.",
                "I find comfort in your company.",
                "Sometimes silence is the best conversation.",
                "Thank you for being here."
            },
            [PersonalityTag.Playful] = new List<string>
            {
                "Catch me if you can!",
                "Bet you can't guess what I'm thinking!",
                "You're so easy to tease!",
                "Want to play a game?",
                "I might let you win... maybe!",
                "You should see the look on your face!",
                "Life's too short to be serious all the time!",
                "I wonder what kind of trouble we can get into today...",
                "You're fun to mess with!",
                "Don't take everything so seriously!"
            }
        };

        /// <summary>
        /// Gets a random dialogue line for the specified personality tag.
        /// </summary>
        public string GetRandomDialogue(PersonalityTag tag)
        {
            if (!_dialoguePool.TryGetValue(tag, out var dialogues))
            {
                return "...";
            }
            var index = Random.Shared.Next(dialogues.Count);
            return dialogues[index];
        }
    }
}
