using EverydayGirlsCompanionCollector.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Data
{
    /// <summary>
    /// Seeds the database with initial girl data for the global pool.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the Girls table with 150+ girls if it's empty.
        /// Call this method from Program.cs or run manually.
        /// </summary>
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.Migrate();

            // Check if Girls table already has data
            if (context.Girls.Any())
            {
                return; // Database already seeded
            }

            var girls = new List<Girl>
            {
                // Set 1: Common names (1-50)
                new Girl { Name = "Emma", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Olivia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ava", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sophia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Isabella", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Mia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Charlotte", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Amelia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Harper", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Evelyn", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Abigail", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Emily", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Luna", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ella", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Lily", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Aria", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Chloe", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Mila", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Hazel", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Grace", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Violet", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Aurora", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Scarlett", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Zoe", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Hannah", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Lucy", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Stella", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nora", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ellie", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ruby", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Maya", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Claire", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Alice", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sophie", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Anna", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Bella", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Elena", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ivy", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nova", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Willow", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Rose", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Iris", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Jade", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Eva", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Jasmine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Daisy", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sarah", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Justine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Olive", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Faith", ImageUrl = "/images/girls/placeholder.jpg" },

                // Set 2: Japanese-inspired names (51-100)
                new Girl { Name = "Sakura", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yuki", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Hana", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Aiko", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Emi", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Mei", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Rin", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sora", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Haru", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Kiko", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Natsuki", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ayame", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Himari", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Koharu", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Mio", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Akari", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yui", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Hinata", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Miyu", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Riko", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nanami", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Kaede", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Misaki", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Asuka", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Shiori", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Momoka", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Rena", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sayuri", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Aoi", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Haruka", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nana", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yuna", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Miku", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Kana", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Chiyo", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nozomi", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Rina", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Suzume", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Chika", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Kotone", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yuzuki", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Reina", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Akane", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Midori", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Sumire", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Tsubaki", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Wakaba", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yoshiko", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Kazumi", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Tomomi", ImageUrl = "/images/girls/placeholder.jpg" },

                // Set 3: European-inspired names (101-150)
                new Girl { Name = "Amelie", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Brigitte", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Celine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Delphine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Estelle", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Francesca", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Giselle", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Helene", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ingrid", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Juliette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Katerina", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Lucia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Margot", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Natasha", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Odette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Petra", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Rosalie", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Svetlana", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Tatiana", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ursula", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Vivienne", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yvette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Anastasia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Beatrice", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Camille", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Dominique", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Eloise", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Frederica", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Genevieve", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Henrietta", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Isabelle", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Josephine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Klara", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Lorraine", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Magdalena", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Nicolette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Ophelia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Pauline", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Roxanne", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Simone", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Therese", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Valencia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Wilhelmina", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Xenia", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Yvonne", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Zara", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Adelaide", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Bernadette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Colette", ImageUrl = "/images/girls/placeholder.jpg" },
                new Girl { Name = "Daphne", ImageUrl = "/images/girls/placeholder.jpg" }
            };

            context.Girls.AddRange(girls);
            context.SaveChanges();
        }
    }
}
