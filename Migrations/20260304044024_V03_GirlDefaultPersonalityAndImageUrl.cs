using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EverydayGirlsCompanionCollector.Migrations
{
    /// <inheritdoc />
    public partial class V03_GirlDefaultPersonalityAndImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultPersonalityTag",
                table: "Girls",
                type: "int",
                nullable: true);

            // Populate DefaultPersonalityTag and new ImageUrl for all existing Girls rows.
            // PersonalityTag enum values: Cheerful=0, Shy=1, Energetic=2, Calm=3,
            //   Playful=4, Tsundere=5, Cool=6, Doting=7, Yandere=8
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Roshidere/Alya.jpg' WHERE Name = 'Alya'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Roshidere/Masha.jpg' WHERE Name = 'Masha'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Evangelion/Asuka Langley.jpg' WHERE Name = 'Asuka Langley'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/ReZero/Emilia.jpg' WHERE Name = 'Emilia'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 7, ImageUrl = '/images/girls/ReZero/Rem.jpg' WHERE Name = 'Rem'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Attack on Titan/Mikasa Ackerman.jpg' WHERE Name = 'Mikasa Ackerman'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 7, ImageUrl = '/images/girls/Roshidere/Yuki Suou.jpg' WHERE Name = 'Yuki Suou'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 7, ImageUrl = '/images/girls/Sword Art Online/Asuna Yuuki.jpg' WHERE Name = 'Asuna Yuuki'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/Sword Art Online/Kirigaya Suguha.jpg' WHERE Name = 'Kirigaya Suguha'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Zenless Zone Zero/Ellen Joe.jpg' WHERE Name = 'Ellen Joe'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Zenless Zone Zero/Qingyi.jpg' WHERE Name = 'Qingyi'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Zenless Zone Zero/Burnice White.jpg' WHERE Name = 'Burnice White'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Zenless Zone Zero/Luciana de Montefio.jpg' WHERE Name = 'Luciana de Montefio'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Zenless Zone Zero/Hoshimi Miyabi.jpg' WHERE Name = 'Hoshimi Miyabi'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Genshin Impact/Keqing.jpg' WHERE Name = 'Keqing'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Genshin Impact/Mona.jpg' WHERE Name = 'Mona'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 3, ImageUrl = '/images/girls/Genshin Impact/Ayaka.jpg' WHERE Name = 'Ayaka'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Genshin Impact/Fischl.jpg' WHERE Name = 'Fischl'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 3, ImageUrl = '/images/girls/Genshin Impact/Raiden Shogun.jpg' WHERE Name = 'Raiden Shogun'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Genshin Impact/Hu Tao.jpg' WHERE Name = 'Hu Tao'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/Chainsaw Man/Power.jpg' WHERE Name = 'Power'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Chainsaw Man/Reze.jpg' WHERE Name = 'Reze'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 8, ImageUrl = '/images/girls/Chainsaw Man/Makima.jpg' WHERE Name = 'Makima'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 1, ImageUrl = '/images/girls/Kimetsu no Yaiba/Tsuyuri Kanao.jpg' WHERE Name = 'Tsuyuri Kanao'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Epic Seven/Rinak.jpg' WHERE Name = 'Rinak'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Epic Seven/Melissa.jpg' WHERE Name = 'Melissa'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Epic Seven/Tenebria.jpg' WHERE Name = 'Tenebria'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Honkai Star Rail/Herta.jpg' WHERE Name = 'Herta'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Honkai Star Rail/Silverwolf.jpg' WHERE Name = 'Silverwolf'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Honkai Star Rail/Sparkle.jpg' WHERE Name = 'Sparkle'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Vocaloid/Hatsune Miku.jpg' WHERE Name = 'Hatsune Miku'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/YuGiOh/Dark Magician Girl.jpg' WHERE Name = 'Dark Magician Girl'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 4, ImageUrl = '/images/girls/Call of the Night/Nanakusa Nazuna.jpg' WHERE Name = 'Nanakusa Nazuna'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/Chaos Zero Nightmare/Mei Lin.jpg' WHERE Name = 'Mei Lin'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Chaos Zero Nightmare/Sereniel.jpg' WHERE Name = 'Sereniel'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Chaos Zero Nightmare/Orlea.jpg' WHERE Name = 'Orlea'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Idolmaster/Shibuya Rin.jpg' WHERE Name = 'Shibuya Rin'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 3, ImageUrl = '/images/girls/Blue Archive/Shiroko.jpg' WHERE Name = 'Shiroko'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Blue Archive/Toki.jpg' WHERE Name = 'Toki'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 8, ImageUrl = '/images/girls/Boku no Hero Academia/Himiko Toga.jpg' WHERE Name = 'Himiko Toga'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/Boku no Hero Academia/Nejire Hado.jpg' WHERE Name = 'Nejire Hado'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Boku no Hero Academia/Toru Hagakure.jpg' WHERE Name = 'Toru Hagakure'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Nagatoro/Nagatoro.jpg' WHERE Name = 'Nagatoro'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 3, ImageUrl = '/images/girls/Frieren/Frieren.jpg' WHERE Name = 'Frieren'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 3, ImageUrl = '/images/girls/Frieren/Fern.jpg' WHERE Name = 'Fern'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 7, ImageUrl = '/images/girls/Stella Sora/Chitose.jpg' WHERE Name = 'Chitose'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/One Punch Man/Tatsumaki.jpg' WHERE Name = 'Tatsumaki'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/My Dressup Darling/Kitagawa Marin.jpg' WHERE Name = 'Kitagawa Marin'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Fate/Artoria Pendragon.jpg' WHERE Name = 'Artoria Pendragon'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Fate/Tohsaka Rin.jpg' WHERE Name = 'Tohsaka Rin'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 2, ImageUrl = '/images/girls/Oshi no Ko/Hoshino Ruby.jpg' WHERE Name = 'Hoshino Ruby'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Oshi no Ko/Hoshino Ai.jpg' WHERE Name = 'Hoshino Ai'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Oshi no Ko/Kurokawa Akane.jpg' WHERE Name = 'Kurokawa Akane'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 5, ImageUrl = '/images/girls/Oshi no Ko/Arima Kana.jpg' WHERE Name = 'Arima Kana'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Apothecary Diaries/Maomao.jpg' WHERE Name = 'Maomao'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Boku no Hero Academia/Uraraka Ochako.jpg' WHERE Name = 'Uraraka Ochako'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 0, ImageUrl = '/images/girls/Lycoris Recoil/Nishikigi Chisato.jpg' WHERE Name = 'Nishikigi Chisato'");
            migrationBuilder.Sql("UPDATE Girls SET DefaultPersonalityTag = 6, ImageUrl = '/images/girls/Lycoris Recoil/Inoue Takina.jpg' WHERE Name = 'Inoue Takina'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPersonalityTag",
                table: "Girls");
            // ImageUrl values are not reverted; reverting to numeric scheme would require
            // the old mapping which is no longer in source. The column remains intact.
        }
    }
}
