using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedScrollProgressTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- EN
(gen_random_uuid(), 'pagination.showingOf', 'en', 'Showing {loaded} of {total}', 'pagination', NOW()),
(gen_random_uuid(), 'pagination.allLoaded', 'en', 'All {total} items loaded', 'pagination', NOW()),
(gen_random_uuid(), 'pagination.loadingMore', 'en', 'Loading...', 'pagination', NOW()),
-- UA
(gen_random_uuid(), 'pagination.showingOf', 'ua', 'Показано {loaded} з {total}', 'pagination', NOW()),
(gen_random_uuid(), 'pagination.allLoaded', 'ua', 'Всі {total} елементів завантажено', 'pagination', NOW()),
(gen_random_uuid(), 'pagination.loadingMore', 'ua', 'Завантаження...', 'pagination', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Translations"" WHERE ""Key"" IN (
    'pagination.showingOf', 'pagination.allLoaded', 'pagination.loadingMore'
);
");
        }
    }
}
