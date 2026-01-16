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
        /// Seeds the Girls table with the available girls if it's empty.
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
                new Girl { Name = "Alya", ImageUrl = "/images/girls/001.jpg" },
                new Girl { Name = "Masha", ImageUrl = "/images/girls/002.jpg" },
                new Girl { Name = "Asuka Langley", ImageUrl = "/images/girls/003.jpg" },
                new Girl { Name = "Emilia", ImageUrl = "/images/girls/004.jpg" },
                new Girl { Name = "Rem", ImageUrl = "/images/girls/005.jpg" },
                new Girl { Name = "Mikasa Ackerman", ImageUrl = "/images/girls/006.jpg" },
                new Girl { Name = "Yuki Suou", ImageUrl = "/images/girls/007.jpg" },
                new Girl { Name = "Asuna Yuuki", ImageUrl = "/images/girls/008.jpg" },
                new Girl { Name = "Kirigaya Suguha", ImageUrl = "/images/girls/009.jpg" },
                new Girl { Name = "Ellen Joe", ImageUrl = "/images/girls/010.jpg" },
                new Girl { Name = "Qingyi", ImageUrl = "/images/girls/011.jpg" },
                new Girl { Name = "Burnice White", ImageUrl = "/images/girls/012.jpg" },
                new Girl { Name = "Luciana de Montefio", ImageUrl = "/images/girls/013.jpg" },
                new Girl { Name = "Hoshimi Miyabi", ImageUrl = "/images/girls/014.jpg" },
                new Girl { Name = "Keqing", ImageUrl = "/images/girls/015.jpg" },
                new Girl { Name = "Mona", ImageUrl = "/images/girls/016.jpg" },
                new Girl { Name = "Ayaka", ImageUrl = "/images/girls/017.jpg" },
                new Girl { Name = "Fischl", ImageUrl = "/images/girls/018.jpg" },
                new Girl { Name = "Raiden Shogun", ImageUrl = "/images/girls/019.jpg" },
                new Girl { Name = "Hu Tao", ImageUrl = "/images/girls/020.jpg" },
                new Girl { Name = "Power", ImageUrl = "/images/girls/021.jpg" },
                new Girl { Name = "Reze", ImageUrl = "/images/girls/022.jpg" },
                new Girl { Name = "Makima", ImageUrl = "/images/girls/023.jpg" },
                new Girl { Name = "Tsuyuri Kanao", ImageUrl = "/images/girls/024.jpg" },
                new Girl { Name = "Rinak", ImageUrl = "/images/girls/025.jpg" },
                new Girl { Name = "Melissa", ImageUrl = "/images/girls/026.jpg" },
                new Girl { Name = "Tenebria", ImageUrl = "/images/girls/027.jpg" },
                new Girl { Name = "Herta", ImageUrl = "/images/girls/028.jpg" },
                new Girl { Name = "Silverwolf", ImageUrl = "/images/girls/029.jpg" },
                new Girl { Name = "Sparkle", ImageUrl = "/images/girls/030.jpg" },
                new Girl { Name = "Hatsune Miku", ImageUrl = "/images/girls/031.jpg" },
                new Girl { Name = "Dark Magician Girl", ImageUrl = "/images/girls/032.jpg" },
                new Girl { Name = "Nanakusa Nazuna", ImageUrl = "/images/girls/033.jpg" },
                new Girl { Name = "Mei Lin", ImageUrl = "/images/girls/034.jpg" },
                new Girl { Name = "Sereniel", ImageUrl = "/images/girls/035.jpg" },
                new Girl { Name = "Tressa", ImageUrl = "/images/girls/036.jpg" },
                new Girl { Name = "Shibuya Rin", ImageUrl = "/images/girls/037.jpg" },
                new Girl { Name = "Shiroko", ImageUrl = "/images/girls/038.jpg" },
                new Girl { Name = "Toki", ImageUrl = "/images/girls/039.jpg" },
                new Girl { Name = "Himiko Toga", ImageUrl = "/images/girls/040.jpg" },
                new Girl { Name = "Nejire Hado", ImageUrl = "/images/girls/041.jpg" },
                new Girl { Name = "Toru Hagakure", ImageUrl = "/images/girls/042.jpg" },
                new Girl { Name = "Nagatoro", ImageUrl = "/images/girls/043.jpg" },
                new Girl { Name = "Frieren", ImageUrl = "/images/girls/044.jpg" },
                new Girl { Name = "Fern", ImageUrl = "/images/girls/045.jpg" },
                new Girl { Name = "Chitose", ImageUrl = "/images/girls/046.jpg" },
                new Girl { Name = "Tatsumaki", ImageUrl = "/images/girls/047.jpg" },
                new Girl { Name = "Kitagawa Marin", ImageUrl = "/images/girls/048.jpg" },
                new Girl { Name = "Artoria Pendragon", ImageUrl = "/images/girls/049.jpg" },
                new Girl { Name = "Tohsaka Rin", ImageUrl = "/images/girls/050.jpg" },
                new Girl { Name = "Hoshino Ruby", ImageUrl = "/images/girls/051.jpg" },
                new Girl { Name = "Hoshino Ai", ImageUrl = "/images/girls/052.jpg" },
                new Girl { Name = "Kurokawa Akane", ImageUrl = "/images/girls/053.jpg" },
                new Girl { Name = "Arima Kana", ImageUrl = "/images/girls/054.jpg" },
                new Girl { Name = "Maomao", ImageUrl = "/images/girls/055.jpg" },
                new Girl { Name = "Uraraka Ochako", ImageUrl = "/images/girls/056.jpg" },
                new Girl { Name = "Nishikigi Chisato", ImageUrl = "/images/girls/057.jpg" },
                new Girl { Name = "Inoue Takina", ImageUrl = "/images/girls/058.jpg" }
            };

            context.Girls.AddRange(girls);
            context.SaveChanges();
        }
    }
}
