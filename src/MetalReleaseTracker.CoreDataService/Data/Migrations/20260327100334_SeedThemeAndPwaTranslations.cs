using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedThemeAndPwaTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- EN
(gen_random_uuid(), 'header.themeTooltip', 'en', 'Toggle theme', 'header', NOW()),
(gen_random_uuid(), 'pwa.installMessage', 'en', 'Install Metal Release Tracker for quick access', 'pwa', NOW()),
(gen_random_uuid(), 'pwa.install', 'en', 'Install', 'pwa', NOW()),
-- UA
(gen_random_uuid(), 'header.themeTooltip', 'ua', 'Змінити тему', 'header', NOW()),
(gen_random_uuid(), 'pwa.installMessage', 'ua', 'Встановіть Metal Release Tracker для швидкого доступу', 'pwa', NOW()),
(gen_random_uuid(), 'pwa.install', 'ua', 'Встановити', 'pwa', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Translations"" WHERE ""Key"" IN ('header.themeTooltip', 'pwa.installMessage', 'pwa.install');
");
        }
    }
}
