using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDistributorSlugAndFixUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Distributors",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.Sql(@"
UPDATE ""Distributors"" SET ""Slug"" = 'black-metal-store' WHERE ""Name"" = 'Black Metal Store';
UPDATE ""Distributors"" SET ""Slug"" = 'black-metal-vendor' WHERE ""Name"" = 'Black Metal Vendor';
UPDATE ""Distributors"" SET ""Slug"" = 'drakkar-records' WHERE ""Name"" = 'Drakkar Records';
UPDATE ""Distributors"" SET ""Slug"" = 'napalm-records' WHERE ""Name"" = 'Napalm Records';
UPDATE ""Distributors"" SET ""Slug"" = 'osmose-productions' WHERE ""Name"" = 'Osmose Productions';
UPDATE ""Distributors"" SET ""Slug"" = 'paragon-records' WHERE ""Name"" = 'Paragon Records';
UPDATE ""Distributors"" SET ""Slug"" = 'season-of-mist' WHERE ""Name"" = 'Season of Mist';
UPDATE ""Distributors"" SET ""Slug"" = LOWER(REGEXP_REPLACE(REGEXP_REPLACE(""Name"", '[^a-zA-Z0-9]+', '-', 'g'), '^-|-$', '', 'g')) WHERE ""Slug"" = '';
");

            migrationBuilder.CreateIndex(
                name: "IX_Distributors_Slug",
                table: "Distributors",
                column: "Slug",
                unique: true);

            migrationBuilder.Sql(@"
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://www.drakkar666.com' WHERE ""Name"" = 'Drakkar Records';
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://www.paragonrecords.org' WHERE ""Name"" = 'Paragon Records';
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://shop.season-of-mist.com' WHERE ""Name"" = 'Season of Mist';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Distributors_Slug",
                table: "Distributors");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Distributors");

            migrationBuilder.Sql(@"
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://www.drakkar.de' WHERE ""Name"" = 'Drakkar Records';
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://www.paragonrecords.com' WHERE ""Name"" = 'Paragon Records';
UPDATE ""Distributors"" SET ""WebsiteUrl"" = 'https://www.season-of-mist.com' WHERE ""Name"" = 'Season of Mist';
");
        }
    }
}
