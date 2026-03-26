using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetalReleaseTracker.CoreDataService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedEmailTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""Translations"" (""Id"", ""Key"", ""Language"", ""Value"", ""Category"", ""UpdatedAt"") VALUES
                -- EN
                (gen_random_uuid(), 'email.title', 'en', 'Email Notifications', 'email', NOW()),
                (gen_random_uuid(), 'email.description', 'en', 'Get notified about price drops and new releases via email', 'email', NOW()),
                (gen_random_uuid(), 'email.inputLabel', 'en', 'Your email address', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribe', 'en', 'Subscribe', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribe', 'en', 'Unsubscribe', 'email', NOW()),
                (gen_random_uuid(), 'email.resend', 'en', 'Resend verification', 'email', NOW()),
                (gen_random_uuid(), 'email.activeFor', 'en', 'Email notifications active for', 'email', NOW()),
                (gen_random_uuid(), 'email.verificationSent', 'en', 'Verification email sent to', 'email', NOW()),
                (gen_random_uuid(), 'email.checkInbox', 'en', 'Check your inbox and click the verification link', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribeSuccess', 'en', 'Verification email sent! Check your inbox.', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribeError', 'en', 'Failed to subscribe. Please try again.', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribeSuccess', 'en', 'Successfully unsubscribed from email notifications.', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribeError', 'en', 'Failed to unsubscribe. Please try again.', 'email', NOW()),
                -- UA
                (gen_random_uuid(), 'email.title', 'ua', 'Email Сповіщення', 'email', NOW()),
                (gen_random_uuid(), 'email.description', 'ua', 'Отримуйте сповіщення про зниження цін та нові релізи на email', 'email', NOW()),
                (gen_random_uuid(), 'email.inputLabel', 'ua', 'Ваша email адреса', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribe', 'ua', 'Підписатися', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribe', 'ua', 'Відписатися', 'email', NOW()),
                (gen_random_uuid(), 'email.resend', 'ua', 'Надіслати повторно', 'email', NOW()),
                (gen_random_uuid(), 'email.activeFor', 'ua', 'Email сповіщення активні для', 'email', NOW()),
                (gen_random_uuid(), 'email.verificationSent', 'ua', 'Лист підтвердження надіслано на', 'email', NOW()),
                (gen_random_uuid(), 'email.checkInbox', 'ua', 'Перевірте вашу пошту та натисніть посилання', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribeSuccess', 'ua', 'Лист підтвердження надіслано!', 'email', NOW()),
                (gen_random_uuid(), 'email.subscribeError', 'ua', 'Не вдалося підписатися.', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribeSuccess', 'ua', 'Ви відписалися від email сповіщень.', 'email', NOW()),
                (gen_random_uuid(), 'email.unsubscribeError', 'ua', 'Не вдалося відписатися.', 'email', NOW())
                ON CONFLICT (""Key"", ""Language"") DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Translations"" WHERE ""Category"" = 'email';");
        }
    }
}
