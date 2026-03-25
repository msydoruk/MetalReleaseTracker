using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendDistributorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Distributors",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryFlag",
                table: "Distributors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Distributors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionUa",
                table: "Distributors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Distributors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Distributors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Distributors" SET
                    "DescriptionEn" = 'French underground metal label founded in 1991 by Hervé Herbaut. Specializes in black and death metal. Based in Lorraine, France.',
                    "DescriptionUa" = 'Французький андерграунд-лейбл, заснований у 1991 році Ерве Ербо. Спеціалізується на блек- та дет-металі. Базується в Лотарингії, Франція.',
                    "Country" = 'France', "CountryFlag" = '🇫🇷',
                    "LogoUrl" = '/logos/osmose.png', "WebsiteUrl" = 'https://www.osmoseproductions.com'
                WHERE LOWER("Name") LIKE '%osmose%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'German heavy metal label founded in 1999 in Witten. Part of Drakkar Entertainment GmbH. Distributes metal releases across Europe.',
                    "DescriptionUa" = 'Німецький метал-лейбл, заснований у 1999 році в Віттені. Частина Drakkar Entertainment GmbH. Дистрибуція метал-релізів по Європі.',
                    "Country" = 'Germany', "CountryFlag" = '🇩🇪',
                    "LogoUrl" = '/logos/drakkar.png', "WebsiteUrl" = 'https://www.drakkar.de'
                WHERE LOWER("Name") LIKE '%drakkar%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'German black metal mailorder shop based in Berlin. Operated by fascination media UG. Offers CDs, vinyl, tapes and merchandise.',
                    "DescriptionUa" = 'Німецький мейлордер-магазин блек-металу з Берліна. fascination media UG. Пропонує CD, вініл, касети та мерч.',
                    "Country" = 'Germany', "CountryFlag" = '🇩🇪',
                    "LogoUrl" = '/logos/black-metal-vendor.png', "WebsiteUrl" = 'https://www.black-metal-vendor.com'
                WHERE LOWER("Name") LIKE '%black metal vendor%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'Brazilian black metal mailorder founded in 1997. Based in São Paulo. Over 5000 titles specializing in black and pagan metal worldwide shipping.',
                    "DescriptionUa" = 'Бразильський мейлордер блек-металу, заснований у 1997 році. Сан-Паулу. Понад 5000 позицій, спеціалізація на блек та паган-металі.',
                    "Country" = 'Brazil', "CountryFlag" = '🇧🇷',
                    "LogoUrl" = '/logos/black-metal-store.webp', "WebsiteUrl" = 'https://www.blackmetalstore.com'
                WHERE LOWER("Name") LIKE '%black metal store%' OR LOWER("Name") LIKE '%blackmetalstore%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'Major Austrian metal label founded in 1992 by Markus Riedler in Eisenerz. One of the largest independent metal labels in the world.',
                    "DescriptionUa" = 'Великий австрійський метал-лейбл, заснований у 1992 році Маркусом Рідлером в Айзенерці. Один з найбільших незалежних метал-лейблів у світі.',
                    "Country" = 'Austria', "CountryFlag" = '🇦🇹',
                    "LogoUrl" = '/logos/napalm-records.png', "WebsiteUrl" = 'https://www.napalmrecords.com'
                WHERE LOWER("Name") LIKE '%napalm%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'French metal label founded in 1996 by Michael S. Berberian in Marseille. Covers black, death, avant-garde and progressive metal.',
                    "DescriptionUa" = 'Французький метал-лейбл, заснований у 1996 році Майклом Берберяном у Марселі. Блек, дет, авангард та прогресивний метал.',
                    "Country" = 'France', "CountryFlag" = '🇫🇷',
                    "LogoUrl" = '/logos/season-of-mist.png', "WebsiteUrl" = 'https://www.season-of-mist.com'
                WHERE LOWER("Name") LIKE '%season of mist%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'American metal label and distributor founded in 2000. Based in New York. Specializes in black, death and doom metal.',
                    "DescriptionUa" = 'Американський метал-лейбл та дистриб''ютор, заснований у 2000 році. Нью-Йорк. Спеціалізується на блек, дет та дум-металі.',
                    "Country" = 'USA', "CountryFlag" = '🇺🇸',
                    "LogoUrl" = '/logos/paragon-records.jpg', "WebsiteUrl" = 'https://www.paragonrecords.com'
                WHERE LOWER("Name") LIKE '%paragon%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'Polish underground label and distro founded in 2007. Werewolf Promotion — Slavonic Forge and Armoury of Underground Heathen Art. Black, pagan and folk metal.',
                    "DescriptionUa" = 'Польський андерграунд-лейбл та дистро, заснований у 2007 році. Werewolf Promotion — Slavonic Forge. Блек, паган та фолк-метал.',
                    "Country" = 'Poland', "CountryFlag" = '🇵🇱',
                    "LogoUrl" = '/logos/werewolf.gif', "WebsiteUrl" = 'https://www.werewolfpromotion.com'
                WHERE LOWER("Name") LIKE '%werewolf%';

                UPDATE "Distributors" SET
                    "DescriptionEn" = 'Italian extreme metal label founded in 1993 by Roberto Mammarella. Based in Milan. Webshop operated via Sound Cave. Black, death and avant-garde metal.',
                    "DescriptionUa" = 'Італійський екстремальний метал-лейбл, заснований у 1993 році. Мілан. Веб-магазин Sound Cave. Блек, дет та авангардний метал.',
                    "Country" = 'Italy', "CountryFlag" = '🇮🇹',
                    "LogoUrl" = '/logos/avantgarde-music.jpg', "WebsiteUrl" = 'https://www.avantgardemusic.com'
                WHERE LOWER("Name") LIKE '%avantgarde%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "CountryFlag",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "DescriptionUa",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Distributors");
        }
    }
}
