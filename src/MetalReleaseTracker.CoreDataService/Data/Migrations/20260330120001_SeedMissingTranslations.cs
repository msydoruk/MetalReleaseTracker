using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedMissingTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
-- compare (EN)
(gen_random_uuid(), 'compare.band', 'en', 'Band', 'compare', NOW()),
(gen_random_uuid(), 'compare.browseAlbums', 'en', 'Browse Albums', 'compare', NOW()),
(gen_random_uuid(), 'compare.buy', 'en', 'Buy', 'compare', NOW()),
(gen_random_uuid(), 'compare.clear', 'en', 'Clear', 'compare', NOW()),
(gen_random_uuid(), 'compare.clearAll', 'en', 'Clear All', 'compare', NOW()),
(gen_random_uuid(), 'compare.compare', 'en', 'Compare', 'compare', NOW()),
(gen_random_uuid(), 'compare.description', 'en', 'Compare album prices and details side by side', 'compare', NOW()),
(gen_random_uuid(), 'compare.distributor', 'en', 'Distributor', 'compare', NOW()),
(gen_random_uuid(), 'compare.empty', 'en', 'No albums to compare', 'compare', NOW()),
(gen_random_uuid(), 'compare.emptyHint', 'en', 'Add albums to compare from the album catalog', 'compare', NOW()),
(gen_random_uuid(), 'compare.media', 'en', 'Format', 'compare', NOW()),
(gen_random_uuid(), 'compare.price', 'en', 'Price', 'compare', NOW()),
(gen_random_uuid(), 'compare.status', 'en', 'Status', 'compare', NOW()),
(gen_random_uuid(), 'compare.title', 'en', 'Compare Albums', 'compare', NOW()),
(gen_random_uuid(), 'compare.year', 'en', 'Year', 'compare', NOW()),
-- email extras (EN)
(gen_random_uuid(), 'email.goToProfile', 'en', 'Go to Profile', 'email', NOW()),
(gen_random_uuid(), 'email.resendError', 'en', 'Failed to resend verification email.', 'email', NOW()),
(gen_random_uuid(), 'email.resendSuccess', 'en', 'Verification email resent successfully.', 'email', NOW()),
(gen_random_uuid(), 'email.verifyError', 'en', 'Verification failed. The link may have expired.', 'email', NOW()),
(gen_random_uuid(), 'email.verifySuccess', 'en', 'Email verified successfully!', 'email', NOW()),
(gen_random_uuid(), 'email.verifying', 'en', 'Verifying your email...', 'email', NOW()),
-- header (EN)
(gen_random_uuid(), 'header.languageTooltip', 'en', 'Change language', 'header', NOW()),
-- news (EN)
(gen_random_uuid(), 'news.empty', 'en', 'No news articles yet.', 'news', NOW()),
-- pagination (EN)
(gen_random_uuid(), 'pagination.endOfList', 'en', 'No more items to load', 'pagination', NOW()),

-- compare (UA)
(gen_random_uuid(), 'compare.band', 'ua', 'Гурт', 'compare', NOW()),
(gen_random_uuid(), 'compare.browseAlbums', 'ua', 'Переглянути альбоми', 'compare', NOW()),
(gen_random_uuid(), 'compare.buy', 'ua', 'Купити', 'compare', NOW()),
(gen_random_uuid(), 'compare.clear', 'ua', 'Очистити', 'compare', NOW()),
(gen_random_uuid(), 'compare.clearAll', 'ua', 'Очистити все', 'compare', NOW()),
(gen_random_uuid(), 'compare.compare', 'ua', 'Порівняти', 'compare', NOW()),
(gen_random_uuid(), 'compare.description', 'ua', 'Порівняйте ціни та деталі альбомів поруч', 'compare', NOW()),
(gen_random_uuid(), 'compare.distributor', 'ua', 'Дистриб''ютор', 'compare', NOW()),
(gen_random_uuid(), 'compare.empty', 'ua', 'Немає альбомів для порівняння', 'compare', NOW()),
(gen_random_uuid(), 'compare.emptyHint', 'ua', 'Додайте альбоми з каталогу для порівняння', 'compare', NOW()),
(gen_random_uuid(), 'compare.media', 'ua', 'Формат', 'compare', NOW()),
(gen_random_uuid(), 'compare.price', 'ua', 'Ціна', 'compare', NOW()),
(gen_random_uuid(), 'compare.status', 'ua', 'Статус', 'compare', NOW()),
(gen_random_uuid(), 'compare.title', 'ua', 'Порівняння альбомів', 'compare', NOW()),
(gen_random_uuid(), 'compare.year', 'ua', 'Рік', 'compare', NOW()),
-- email extras (UA)
(gen_random_uuid(), 'email.goToProfile', 'ua', 'Перейти до профілю', 'email', NOW()),
(gen_random_uuid(), 'email.resendError', 'ua', 'Не вдалося повторно надіслати лист підтвердження.', 'email', NOW()),
(gen_random_uuid(), 'email.resendSuccess', 'ua', 'Лист підтвердження успішно надіслано повторно.', 'email', NOW()),
(gen_random_uuid(), 'email.verifyError', 'ua', 'Підтвердження не вдалося. Посилання могло закінчитися.', 'email', NOW()),
(gen_random_uuid(), 'email.verifySuccess', 'ua', 'Електронну пошту успішно підтверджено!', 'email', NOW()),
(gen_random_uuid(), 'email.verifying', 'ua', 'Підтвердження вашої електронної пошти...', 'email', NOW()),
-- header (UA)
(gen_random_uuid(), 'header.languageTooltip', 'ua', 'Змінити мову', 'header', NOW()),
-- news (UA)
(gen_random_uuid(), 'news.empty', 'ua', 'Новин ще немає.', 'news', NOW()),
-- pagination (UA)
(gen_random_uuid(), 'pagination.endOfList', 'ua', 'Більше елементів немає', 'pagination', NOW())
ON CONFLICT (""Key"", ""Language"") DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Translations"" WHERE ""Key"" IN (
    'compare.band', 'compare.browseAlbums', 'compare.buy', 'compare.clear', 'compare.clearAll',
    'compare.compare', 'compare.description', 'compare.distributor', 'compare.empty', 'compare.emptyHint',
    'compare.media', 'compare.price', 'compare.status', 'compare.title', 'compare.year',
    'email.goToProfile', 'email.resendError', 'email.resendSuccess', 'email.verifyError',
    'email.verifySuccess', 'email.verifying',
    'header.languageTooltip',
    'news.empty',
    'pagination.endOfList'
);
");
        }
    }
}
