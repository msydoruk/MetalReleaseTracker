namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;

public static class AdminRouteConstants
{
    private const string Base = "api/admin";

    public static class Auth
    {
        public const string Login = $"{Base}/auth/login";
    }

    public static class Dashboard
    {
        public const string Stats = $"{Base}/dashboard/stats";
    }

    public static class Distributors
    {
        private const string Prefix = $"{Base}/distributors";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
        public const string Create = Prefix;
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string Delete = $"{Prefix}/{{id:guid}}";
    }

    public static class Bands
    {
        private const string Prefix = $"{Base}/bands";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string Merge = $"{Prefix}/merge";
        public const string Delete = $"{Prefix}/{{id:guid}}";
    }

    public static class Albums
    {
        private const string Prefix = $"{Base}/albums";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string Delete = $"{Prefix}/{{id:guid}}";
        public const string BulkStatus = $"{Prefix}/bulk-status";
    }

    public static class Currencies
    {
        private const string Prefix = $"{Base}/currencies";
        public const string GetAll = Prefix;
        public const string Create = Prefix;
        public const string Update = $"{Prefix}/{{id:guid}}";
    }

    public static class Navigation
    {
        private const string Prefix = $"{Base}/navigation";
        public const string GetAll = Prefix;
        public const string Create = Prefix;
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string Delete = $"{Prefix}/{{id:guid}}";
    }

    public static class Translations
    {
        private const string Prefix = $"{Base}/translations";
        public const string GetAll = Prefix;
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string BulkUpdate = $"{Prefix}/bulk";
    }

    public static class News
    {
        private const string Prefix = $"{Base}/news";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
        public const string Create = Prefix;
        public const string Update = $"{Prefix}/{{id:guid}}";
        public const string Delete = $"{Prefix}/{{id:guid}}";
    }

    public static class Users
    {
        private const string Prefix = $"{Base}/users";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id}}";
        public const string UpdateRole = $"{Prefix}/{{id}}/role";
        public const string Lock = $"{Prefix}/{{id}}/lock";
        public const string Unlock = $"{Prefix}/{{id}}/unlock";
    }

    public static class Reviews
    {
        private const string Prefix = $"{Base}/reviews";
        public const string GetAll = Prefix;
        public const string Delete = $"{Prefix}/{{id:guid}}";
    }

    public static class Settings
    {
        private const string Prefix = $"{Base}/settings";
        public const string GetByCategory = $"{Prefix}/{{category}}";
        public const string UpdateByCategory = $"{Prefix}/{{category}}";
    }

    public static class Notifications
    {
        private const string Prefix = $"{Base}/notifications";
        public const string Stats = $"{Prefix}/stats";
        public const string Broadcast = $"{Prefix}/broadcast";
    }

    public static class Telegram
    {
        private const string Prefix = $"{Base}/telegram";
        public const string Stats = $"{Prefix}/stats";
        public const string LinkedUsers = $"{Prefix}/linked-users";
    }

    public static class AiSeo
    {
        private const string Prefix = $"{Base}/ai-seo";
        public const string GenerateBand = $"{Prefix}/band/{{id:guid}}";
        public const string GenerateAlbum = $"{Prefix}/album/{{id:guid}}";
        public const string BulkBands = $"{Prefix}/bulk/bands";
        public const string BulkAlbums = $"{Prefix}/bulk/albums";
    }
}
