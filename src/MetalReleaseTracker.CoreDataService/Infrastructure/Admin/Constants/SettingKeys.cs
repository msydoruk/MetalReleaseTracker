namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;

public static class SettingKeys
{
    public static class Authentication
    {
        public const string JwtExpiresMinutes = "JwtExpiresMinutes";
        public const string RefreshTokenExpirationDays = "RefreshTokenExpirationDays";
        public const string PasswordMinLength = "PasswordMinLength";
        public const string PasswordRequireDigit = "PasswordRequireDigit";
        public const string PasswordRequireUppercase = "PasswordRequireUppercase";
        public const string PasswordRequireLowercase = "PasswordRequireLowercase";
        public const string PasswordRequireNonAlphanumeric = "PasswordRequireNonAlphanumeric";
        public const string LockoutTimeSpanMinutes = "LockoutTimeSpanMinutes";
        public const string MaxFailedAccessAttempts = "MaxFailedAccessAttempts";
    }

    public static class Pagination
    {
        public const string DefaultPageSize = "DefaultPageSize";
        public const string MaxPageSize = "MaxPageSize";
    }

    public static class Storage
    {
        public const string PresignedUrlExpiryDays = "PresignedUrlExpiryDays";
    }

    public static class FeatureToggles
    {
        public const string TelegramBotEnabled = "TelegramBotEnabled";
        public const string NotificationsEnabled = "NotificationsEnabled";
        public const string ReviewsEnabled = "ReviewsEnabled";
        public const string RegistrationEnabled = "RegistrationEnabled";
        public const string GoogleAuthEnabled = "GoogleAuthEnabled";
    }

    public static class Telegram
    {
        public const string LinkTokenExpiryMinutes = "LinkTokenExpiryMinutes";
    }

    public static class Notifications
    {
        public const string PriceDropEnabled = "PriceDropEnabled";
        public const string BackInStockEnabled = "BackInStockEnabled";
        public const string RestockEnabled = "RestockEnabled";
        public const string NewVariantEnabled = "NewVariantEnabled";
    }
}
