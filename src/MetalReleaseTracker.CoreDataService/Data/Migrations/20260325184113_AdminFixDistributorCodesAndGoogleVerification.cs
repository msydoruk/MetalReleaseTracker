using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdminFixDistributorCodesAndGoogleVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Distributors" SET "Code" = 9 WHERE "Name" = 'Avantgarde Music';
                UPDATE "Distributors" SET "Code" = 4 WHERE "Name" = 'Black Metal Store';
                UPDATE "Distributors" SET "Code" = 3 WHERE "Name" = 'Black Metal Vendor';
                UPDATE "Distributors" SET "Code" = 2 WHERE "Name" = 'Drakkar Records';
                UPDATE "Distributors" SET "Code" = 5 WHERE "Name" = 'Napalm Records';
                UPDATE "Distributors" SET "Code" = 1 WHERE "Name" = 'Osmose Productions';
                UPDATE "Distributors" SET "Code" = 7 WHERE "Name" = 'Paragon Records';
                UPDATE "Distributors" SET "Code" = 6 WHERE "Name" = 'Season of Mist';
                UPDATE "Distributors" SET "Code" = 8 WHERE "Name" = 'Werewolf';
                """);

            migrationBuilder.Sql("""
                UPDATE "Settings"
                SET "Value" = 'cwZlE3hwgiJ3ICn3rPVZrddw_eX8Nuo4PdUeoQmrIhA', "UpdatedAt" = NOW()
                WHERE "Key" = 'GoogleSiteVerification' AND "Category" = 'Seo';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
