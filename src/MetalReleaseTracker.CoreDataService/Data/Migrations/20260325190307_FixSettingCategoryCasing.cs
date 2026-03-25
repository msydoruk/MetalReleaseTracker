using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSettingCategoryCasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Settings" SET "Category" = 'Authentication' WHERE "Category" = 'authentication';
                UPDATE "Settings" SET "Category" = 'Pagination' WHERE "Category" = 'pagination';
                UPDATE "Settings" SET "Category" = 'Storage' WHERE "Category" = 'storage';
                UPDATE "Settings" SET "Category" = 'FeatureToggles' WHERE "Category" = 'featureToggles';
                UPDATE "Settings" SET "Category" = 'Telegram' WHERE "Category" = 'telegram';
                UPDATE "Settings" SET "Category" = 'Notifications' WHERE "Category" = 'notifications';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
