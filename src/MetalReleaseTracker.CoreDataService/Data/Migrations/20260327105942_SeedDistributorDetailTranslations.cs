using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDistributorDetailTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- EN
(gen_random_uuid(), 'distributorDetail.notFound', 'en', 'Distributor not found', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.backToDistributors', 'en', 'Back to Distributors', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.releases', 'en', 'releases', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.visitWebsite', 'en', 'Visit Website', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.albumsFrom', 'en', 'Releases from {distributorName}', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.noAlbums', 'en', 'No albums available from this distributor yet', 'distributorDetail', NOW()),
(gen_random_uuid(), 'share.distributorText', 'en', 'Check out {distributorName} on Metal Release Tracker', 'share', NOW()),
-- UA
(gen_random_uuid(), 'distributorDetail.notFound', 'ua', 'Дистриб''ютора не знайдено', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.backToDistributors', 'ua', 'Назад до дистриб''юторів', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.releases', 'ua', 'релізів', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.visitWebsite', 'ua', 'Перейти на сайт', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.albumsFrom', 'ua', 'Релізи від {distributorName}', 'distributorDetail', NOW()),
(gen_random_uuid(), 'distributorDetail.noAlbums', 'ua', 'Поки що немає альбомів від цього дистриб''ютора', 'distributorDetail', NOW()),
(gen_random_uuid(), 'share.distributorText', 'ua', 'Подивіться {distributorName} на Metal Release Tracker', 'share', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Translations"" WHERE ""Key"" IN (
    'distributorDetail.notFound', 'distributorDetail.backToDistributors',
    'distributorDetail.releases', 'distributorDetail.visitWebsite',
    'distributorDetail.albumsFrom', 'distributorDetail.noAlbums',
    'share.distributorText'
);
");
        }
    }
}
