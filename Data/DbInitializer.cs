using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EverydayGirlsCompanionCollector.Data
{
    /// <summary>
    /// Seeds the database with initial configuration and girl data.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the database with initial data if tables are empty.
        /// Call this method from Program.cs or run manually.
        /// </summary>
        public static void Initialize(ApplicationDbContext context)
        {
            // Assumes database migrations have already been applied by the caller.

            // Seed TownLocations (idempotent)
            SeedTownLocations(context);

            // Seed Girls (idempotent)
            SeedGirls(context);
        }

        /// <summary>
        /// Seeds the TownLocations table with default locations if empty.
        /// </summary>
        private static void SeedTownLocations(ApplicationDbContext context)
        {
            if (context.TownLocations.Any())
            {
                return; // Already seeded
            }

            var locations = new List<TownLocation>
            {
                // Charm locations (social/relationship-focused)
                new TownLocation
                {
                    Name = "Café",
                    PrimarySkill = SkillType.Charm,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = false,
                    UnlockCost = 0
                },
                new TownLocation
                {
                    Name = "Boutique",
                    PrimarySkill = SkillType.Charm,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = true,
                    UnlockCost = 50
                },

                // Focus locations (study/learning-focused)
                new TownLocation
                {
                    Name = "Library",
                    PrimarySkill = SkillType.Focus,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = false,
                    UnlockCost = 0
                },
                new TownLocation
                {
                    Name = "Observatory",
                    PrimarySkill = SkillType.Focus,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = true,
                    UnlockCost = 50
                },

                // Vitality locations (fitness/activity-focused)
                new TownLocation
                {
                    Name = "Park",
                    PrimarySkill = SkillType.Vitality,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = false,
                    UnlockCost = 0
                },
                new TownLocation
                {
                    Name = "Gym",
                    PrimarySkill = SkillType.Vitality,
                    BaseDailyBondGain = 1,
                    BaseDailyCurrencyGain = 5,
                    BaseDailySkillGain = 10,
                    IsLockedByDefault = true,
                    UnlockCost = 50
                }
            };

            context.TownLocations.AddRange(locations);
            context.SaveChanges();
        }

        /// <summary>
        /// Seeds the Girls table with the available girls if empty.
        /// </summary>
        private static void SeedGirls(ApplicationDbContext context)
        {
            if (context.Girls.Any())
            {
                return; // Database already seeded
            }

            var girls = new List<Girl>
            {
                new Girl { Name = "Alya", ImageUrl = "/images/girls/Roshidere/Alya.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Masha", ImageUrl = "/images/girls/Roshidere/Masha.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Asuka Langley", ImageUrl = "/images/girls/Evangelion/Asuka Langley.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Emilia", ImageUrl = "/images/girls/ReZero/Emilia.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Rem", ImageUrl = "/images/girls/ReZero/Rem.jpg", DefaultPersonalityTag = PersonalityTag.Doting },
                new Girl { Name = "Mikasa Ackerman", ImageUrl = "/images/girls/Attack on Titan/Mikasa Ackerman.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Yuki Suou", ImageUrl = "/images/girls/Roshidere/Yuki Suou.jpg", DefaultPersonalityTag = PersonalityTag.Doting },
                new Girl { Name = "Asuna Yuuki", ImageUrl = "/images/girls/Sword Art Online/Asuna Yuuki.jpg", DefaultPersonalityTag = PersonalityTag.Doting },
                new Girl { Name = "Kirigaya Suguha", ImageUrl = "/images/girls/Sword Art Online/Kirigaya Suguha.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Ellen Joe", ImageUrl = "/images/girls/Zenless Zone Zero/Ellen Joe.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Qingyi", ImageUrl = "/images/girls/Zenless Zone Zero/Qingyi.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Burnice White", ImageUrl = "/images/girls/Zenless Zone Zero/Burnice White.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Luciana de Montefio", ImageUrl = "/images/girls/Zenless Zone Zero/Luciana de Montefio.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Hoshimi Miyabi", ImageUrl = "/images/girls/Zenless Zone Zero/Hoshimi Miyabi.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Keqing", ImageUrl = "/images/girls/Genshin Impact/Keqing.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Mona", ImageUrl = "/images/girls/Genshin Impact/Mona.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Ayaka", ImageUrl = "/images/girls/Genshin Impact/Ayaka.jpg", DefaultPersonalityTag = PersonalityTag.Calm },
                new Girl { Name = "Fischl", ImageUrl = "/images/girls/Genshin Impact/Fischl.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Raiden Shogun", ImageUrl = "/images/girls/Genshin Impact/Raiden Shogun.jpg", DefaultPersonalityTag = PersonalityTag.Calm },
                new Girl { Name = "Hu Tao", ImageUrl = "/images/girls/Genshin Impact/Hu Tao.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Power", ImageUrl = "/images/girls/Chainsaw Man/Power.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Reze", ImageUrl = "/images/girls/Chainsaw Man/Reze.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Makima", ImageUrl = "/images/girls/Chainsaw Man/Makima.jpg", DefaultPersonalityTag = PersonalityTag.Yandere },
                new Girl { Name = "Tsuyuri Kanao", ImageUrl = "/images/girls/Kimetsu no Yaiba/Tsuyuri Kanao.jpg", DefaultPersonalityTag = PersonalityTag.Shy },
                new Girl { Name = "Rinak", ImageUrl = "/images/girls/Epic Seven/Rinak.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Melissa", ImageUrl = "/images/girls/Epic Seven/Melissa.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Tenebria", ImageUrl = "/images/girls/Epic Seven/Tenebria.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Herta", ImageUrl = "/images/girls/Honkai Star Rail/Herta.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Silverwolf", ImageUrl = "/images/girls/Honkai Star Rail/Silverwolf.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Sparkle", ImageUrl = "/images/girls/Honkai Star Rail/Sparkle.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Hatsune Miku", ImageUrl = "/images/girls/Vocaloid/Hatsune Miku.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Dark Magician Girl", ImageUrl = "/images/girls/YuGiOh/Dark Magician Girl.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Nanakusa Nazuna", ImageUrl = "/images/girls/Call of the Night/Nanakusa Nazuna.jpg", DefaultPersonalityTag = PersonalityTag.Playful },
                new Girl { Name = "Mei Lin", ImageUrl = "/images/girls/Chaos Zero Nightmare/Mei Lin.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Sereniel", ImageUrl = "/images/girls/Chaos Zero Nightmare/Sereniel.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Orlea", ImageUrl = "/images/girls/Chaos Zero Nightmare/Orlea.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Shibuya Rin", ImageUrl = "/images/girls/Idolmaster/Shibuya Rin.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Shiroko", ImageUrl = "/images/girls/Blue Archive/Shiroko.jpg", DefaultPersonalityTag = PersonalityTag.Calm },
                new Girl { Name = "Toki", ImageUrl = "/images/girls/Blue Archive/Toki.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Himiko Toga", ImageUrl = "/images/girls/Boku no Hero Academia/Himiko Toga.jpg", DefaultPersonalityTag = PersonalityTag.Yandere },
                new Girl { Name = "Nejire Hado", ImageUrl = "/images/girls/Boku no Hero Academia/Nejire Hado.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Toru Hagakure", ImageUrl = "/images/girls/Boku no Hero Academia/Toru Hagakure.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Nagatoro", ImageUrl = "/images/girls/Nagatoro/Nagatoro.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Frieren", ImageUrl = "/images/girls/Frieren/Frieren.jpg", DefaultPersonalityTag = PersonalityTag.Calm },
                new Girl { Name = "Fern", ImageUrl = "/images/girls/Frieren/Fern.jpg", DefaultPersonalityTag = PersonalityTag.Calm },
                new Girl { Name = "Chitose", ImageUrl = "/images/girls/Stella Sora/Chitose.jpg", DefaultPersonalityTag = PersonalityTag.Doting },
                new Girl { Name = "Tatsumaki", ImageUrl = "/images/girls/One Punch Man/Tatsumaki.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Kitagawa Marin", ImageUrl = "/images/girls/My Dressup Darling/Kitagawa Marin.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Artoria Pendragon", ImageUrl = "/images/girls/Fate/Artoria Pendragon.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Tohsaka Rin", ImageUrl = "/images/girls/Fate/Tohsaka Rin.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Hoshino Ruby", ImageUrl = "/images/girls/Oshi no Ko/Hoshino Ruby.jpg", DefaultPersonalityTag = PersonalityTag.Energetic },
                new Girl { Name = "Hoshino Ai", ImageUrl = "/images/girls/Oshi no Ko/Hoshino Ai.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Kurokawa Akane", ImageUrl = "/images/girls/Oshi no Ko/Kurokawa Akane.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Arima Kana", ImageUrl = "/images/girls/Oshi no Ko/Arima Kana.jpg", DefaultPersonalityTag = PersonalityTag.Tsundere },
                new Girl { Name = "Maomao", ImageUrl = "/images/girls/Apothecary Diaries/Maomao.jpg", DefaultPersonalityTag = PersonalityTag.Cool },
                new Girl { Name = "Uraraka Ochako", ImageUrl = "/images/girls/Boku no Hero Academia/Uraraka Ochako.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Nishikigi Chisato", ImageUrl = "/images/girls/Lycoris Recoil/Nishikigi Chisato.jpg", DefaultPersonalityTag = PersonalityTag.Cheerful },
                new Girl { Name = "Inoue Takina", ImageUrl = "/images/girls/Lycoris Recoil/Inoue Takina.jpg", DefaultPersonalityTag = PersonalityTag.Cool }
            };

            context.Girls.AddRange(girls);
            context.SaveChanges();
        }
    }
}
