using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedChangelogReasonTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- EN
(gen_random_uuid(), 'changelog.change', 'en', 'Change', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonPriceDown', 'en', 'Price ↓', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonPriceUp', 'en', 'Price ↑', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonStatusChange', 'en', 'Status', 'changelog', NOW()),
-- UA
(gen_random_uuid(), 'changelog.change', 'ua', 'Зміна', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonPriceDown', 'ua', 'Ціна ↓', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonPriceUp', 'ua', 'Ціна ↑', 'changelog', NOW()),
(gen_random_uuid(), 'changelog.reasonStatusChange', 'ua', 'Статус', 'changelog', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Translations"" WHERE ""Key"" IN (
    'changelog.change', 'changelog.reasonPriceDown', 'changelog.reasonPriceUp', 'changelog.reasonStatusChange'
);
");
        }
    }
}
