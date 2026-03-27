using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedRateLimitingAndShareTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Settings"" (""Key"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
('CatalogPermitLimit', '100', 'RateLimiting', NOW()),
('CatalogWindowMinutes', '1', 'RateLimiting', NOW()),
('AuthPermitLimit', '10', 'RateLimiting', NOW()),
('AuthWindowMinutes', '1', 'RateLimiting', NOW())
ON CONFLICT (""Key"") DO NOTHING;
");

            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- EN
(gen_random_uuid(), 'share.share', 'en', 'Share', 'share', NOW()),
(gen_random_uuid(), 'share.twitter', 'en', 'Twitter / X', 'share', NOW()),
(gen_random_uuid(), 'share.facebook', 'en', 'Facebook', 'share', NOW()),
(gen_random_uuid(), 'share.telegram', 'en', 'Telegram', 'share', NOW()),
(gen_random_uuid(), 'share.copyLink', 'en', 'Copy Link', 'share', NOW()),
(gen_random_uuid(), 'share.linkCopied', 'en', 'Link copied!', 'share', NOW()),
(gen_random_uuid(), 'share.albumText', 'en', '{bandName} - {albumName} on Metal Release Tracker', 'share', NOW()),
(gen_random_uuid(), 'share.bandText', 'en', '{bandName} on Metal Release Tracker', 'share', NOW()),
-- UA
(gen_random_uuid(), 'share.share', 'ua', 'Поділитися', 'share', NOW()),
(gen_random_uuid(), 'share.twitter', 'ua', 'Twitter / X', 'share', NOW()),
(gen_random_uuid(), 'share.facebook', 'ua', 'Facebook', 'share', NOW()),
(gen_random_uuid(), 'share.telegram', 'ua', 'Telegram', 'share', NOW()),
(gen_random_uuid(), 'share.copyLink', 'ua', 'Копіювати посилання', 'share', NOW()),
(gen_random_uuid(), 'share.linkCopied', 'ua', 'Посилання скопійовано!', 'share', NOW()),
(gen_random_uuid(), 'share.albumText', 'ua', '{bandName} - {albumName} на Metal Release Tracker', 'share', NOW()),
(gen_random_uuid(), 'share.bandText', 'ua', '{bandName} на Metal Release Tracker', 'share', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Settings"" WHERE ""Category"" = 'RateLimiting';");
            migrationBuilder.Sql(@"DELETE FROM ""Translations"" WHERE ""Category"" = 'share';");
        }
    }
}
