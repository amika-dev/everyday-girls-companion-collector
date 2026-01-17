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
                "Life is so much more fun with you!",
                "I was looking forward to seeing you!",
                "Just being here with you makes me feel happy!",
                "Good things always happen when we're together!",
                "You bring out the best in me!",
                "Spending time with you makes everything brighter!",
                "I hope we have lots of days like this!",
                "I feel like I can do anything with you around!",
                "Thanks for always cheering me up!",
                "Today feels extra nice somehow!",
                "I'm really glad you came to see me!"
            },
            [PersonalityTag.Shy] = new List<string>
            {
                "I... I'm glad you're here...",
                "Thanks for spending time with me...",
                "I feel safe when you're around...",
                "You're really kind...",
                "I hope I'm not bothering you...",
                "This is... nice...",
                "I've been thinking about you...",
                "You make me feel comfortable...",
                "I'm happy we can be together like this...",
                "Um... thanks for coming today...",
                "I don't talk much, but I like being with you...",
                "You always make me feel a little braver...",
                "I was hoping I'd get to see you...",
                "Being with you helps me relax...",
                "I feel calmer when you're nearby...",
                "I’m not very good with words, but... thank you...",
                "Even quiet days feel special with you...",
                "I’m glad you didn’t forget about me...",
                "I enjoy these little moments together...",
                "I hope we can do this again soon..."
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
                "Let's make this the best day ever!",
                "Maybe we could run a marathon together!",
                "Staying still is impossible today!",
                "Being with you gives me extra energy!",
                "Come on, keep up with me!",
                "There's so much we could do together!",
                "I hope you're ready for a fun day!",
                "Let's go on an adventure today!",
                "Let’s make some great memories!",
                "I could talk to you all day!",
                "Every day with you is an adventure!"
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
                "Thank you for being here.",
                "Days like this are my favorite.",
                "There’s no need to rush anything.",
                "Spending time together like this feels nice.",
                "I feel at ease when you're around.",
                "Even ordinary days feel pleasant with you.",
                "It's nice to slow down and just be together.",
                "Moments like this are precious.",
                "I’m glad we can share a calm day.",
                "Everything feels a little gentler today.",
                "Your company brings me peace.",
                "I wouldn’t mind if time stayed like this.",
                "Thank you for this quiet moment."
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
                "Don't take everything so seriously!",
                "I might be up to something sneaky!",
                "Try to keep up with me!",
                "I can't help teasing you a little!",
                "You make it too easy sometimes!",
                "Can you guess what I'm going to do next?",
                "Spending time with you is never boring!",
                "I’ve got a few tricks up my sleeve!",
                "Relax a little and have some fun!",
                "I like seeing you flustered!",
                "I think you'll remember today!",
                "Hey, guess what I did earlier."
            },
            [PersonalityTag.Tsundere] = new List<string>
            {
                "I-I didn’t miss you or anything...",
                "Don’t get the wrong idea, okay?",
                "I’m only here because I had nothing better to do!",
                "You don’t have to come see me every day, you know...",
                "Hmph. I guess spending time with you is fine.",
                "It’s not like I was waiting for you!",
                "You should feel lucky I’m talking to you.",
                "I suppose today wasn’t completely terrible...",
                "You’re surprisingly not annoying today.",
                "Don’t read too much into this!",
                "I’m only being nice because I feel like it!",
                "You better not start getting smug.",
                "W-Whatever, I had a decent time...",
                "Try not to get the wrong impression, okay?",
                "I don’t hate being around you... I guess.",
                "You can stay a little longer if you want.",
                "At least you kept me entertained... I guess.",
                "Don’t act so happy just because I showed up!",
                "You’re not as bad as I thought.",
                "Just don’t make this a habit or anything!"
            },
            [PersonalityTag.Cool] = new List<string>
            {
                "You’re not bad company.",
                "You’re easier to talk to than most people.",
                "I had a decent time.",
                "It was a good way to spend the day.",
                "I don’t say it often, but thank you.",
                "I find our conversations interesting.",
                "You have a way of keeping things simple.",
                "This kind of day suits me.",
                "I could get used to this.",
                "Not every moment needs excitement.",
                "You’re more perceptive than you look.",
                "I’ll admit, today wasn’t bad.",
                "Feel free to come by again.",
                "Taking things slow is my style",
                "I like things calm and uncomplicated.",
                "Some days are better when you keep it simple.",
                "I prefer to let things flow naturally.",
                "There's no need to rush through life.",
                "Once in a while, it's nice to just chill.",
                "Easygoing days are my favorite."
            },
            [PersonalityTag.Doting] = new List<string>
            {
                "Did you have a good day today?",
                "You look tired. Maybe I could give you a massage.",
                "You’re doing a great job, you know.",
                "I’m glad I can be here for you.",
                "You can relax when you’re with me.",
                "Make sure you’re taking care of yourself.",
                "I enjoy looking after you a little.",
                "You don’t have to carry everything alone.",
                "You can lay in my lap if you need to rest.",
                "I’m proud of how hard you've been working.",
                "Come here, let me take care of you today.",
                "You can always rely on me.",
                "I’m happy just knowing you’re doing well.",
                "Take your time — there’s no need to hurry.",
                "You’ve been working hard, haven’t you?",
                "Let me know if there’s anything you need.",
                "I’ll always have time for you.",
                "You’re important to me, you know.",
                "I hope you feel safe when you’re with me.",
                "I’ll be cheering you on, always."
            },
            [PersonalityTag.Yandere] = new List<string>
            {
                "I’m so happy you chose to spend time with just me today.",
                "You don’t need anyone else when you have me.",
                "I was waiting for you... just you.",
                "No one understands you like I do.",
                "I hope you only think about me today.",
                "Spending time together with just you feels perfect.",
                "I don’t like it when others take your attention.",
                "You’re the only one I really care about.",
                "It's a lot more calm when it's just the two of us, right?",
                "I hope you’ll always stay close to me.",
                "You wouldn’t forget about me... right?",
                "I'm the most important to you aren't I?",
                "I noticed that girl was talking to you again today... Do I need to speak with her?",
                "I’m happiest when you’re righttt here.",
                "You don’t need anyone but me, do you?",
                "I’ll always be here for you... always.",
                "I like knowing exactly where you are every day.",
                "You'll keep sharing your time with just me, won't you?",
                "I've been thinking about moving in with you... just kidding",
                "Don’t worry… I always remember everything about you."
            },
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
