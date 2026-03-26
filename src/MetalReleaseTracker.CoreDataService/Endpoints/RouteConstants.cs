namespace MetalReleaseTracker.CoreDataService.Endpoints;

public static class RouteConstants
{
    public static class Api
    {
        private const string ApiBase = "api";

        public static class Auth
        {
            private const string Base = $"{ApiBase}/auth";
            public const string LoginWithEmail = $"{Base}/login/email";
            public const string Register = $"{Base}/register";
            public const string Logout = $"{Base}/logout";
            public const string RefreshToken = $"{Base}/refresh-token";
            public const string RevokeToken = $"{Base}/revoke-token";
            public const string GoogleLogin = $"{Base}/google-login";
            public const string GoogleAuthComplete = $"{Base}/google-auth-complete";
        }

        public static class Albums
        {
            private const string Base = $"{ApiBase}/albums";
            public const string GetFiltered = $"{Base}/filtered";
            public const string GetGrouped = $"{Base}/grouped";
            public const string GetSuggestions = $"{Base}/suggest";
            public const string GetById = $"{Base}/{{id:guid}}";
            public const string GetDetail = $"{Base}/{{id:guid}}/detail";
            public const string GetBySlug = $"{Base}/by-slug/{{slug}}";
            public const string GetDetailBySlug = $"{Base}/by-slug/{{slug}}/detail";
        }

        public static class Bands
        {
            private const string Base = $"{ApiBase}/bands";
            public const string GetAll = $"{Base}/all";
            public const string GetById = $"{Base}/{{id:guid}}";
            public const string GetWithAlbumCount = $"{Base}/with-album-count";
            public const string GetGenres = $"{Base}/genres";
            public const string GetSimilar = $"{Base}/{{id:guid}}/similar";
            public const string GetBySlug = $"{Base}/by-slug/{{slug}}";
        }

        public static class Distributors
        {
            private const string Base = $"{ApiBase}/distributors";
            public const string GetAll = $"{Base}/all";
            public const string GetById = $"{Base}/{{id:guid}}";
            public const string GetWithAlbumCount = $"{Base}/with-album-count";
        }

        public static class Reviews
        {
            private const string Base = $"{ApiBase}/reviews";
            public const string GetAll = Base;
            public const string Submit = Base;
        }

        public static class ChangeLog
        {
            private const string Base = $"{ApiBase}/changelog";
            public const string GetPaged = Base;
            public const string GetPriceHistory = $"{Base}/price-history";
        }

        public static class Feed
        {
            private const string Base = $"{ApiBase}/feed";
            public const string Rss = $"{Base}/rss";
        }

        public static class Ratings
        {
            private const string Base = $"{ApiBase}/ratings";
            public const string Submit = $"{Base}/{{albumId:guid}}";
            public const string Get = $"{Base}/{{albumId:guid}}";
            public const string Delete = $"{Base}/{{albumId:guid}}";
        }

        public static class Favorites
        {
            private const string Base = $"{ApiBase}/favorites";
            public const string Add = $"{Base}/{{albumId:guid}}";
            public const string Remove = $"{Base}/{{albumId:guid}}";
            public const string GetAll = Base;
            public const string GetIds = $"{Base}/ids";
            public const string Check = $"{Base}/{{albumId:guid}}/check";
            public const string UpdateStatus = $"{Base}/{{albumId:guid}}/status";
            public const string Export = $"{Base}/export";
        }

        public static class FollowedBands
        {
            private const string Base = $"{ApiBase}/followed-bands";
            public const string Follow = $"{Base}/{{bandId:guid}}";
            public const string Unfollow = $"{Base}/{{bandId:guid}}";
            public const string GetAll = Base;
            public const string GetIds = $"{Base}/ids";
            public const string Check = $"{Base}/{{bandId:guid}}/check";
            public const string Feed = $"{Base}/feed";
            public const string FollowerCount = $"{Base}/{{bandId:guid}}/count";
        }

        public static class Watches
        {
            private const string Base = $"{ApiBase}/watches";
            public const string Watch = $"{Base}/{{albumId:guid}}";
            public const string Unwatch = $"{Base}/{{albumId:guid}}";
            public const string Check = $"{Base}/{{albumId:guid}}/check";
            public const string GetKeys = $"{Base}/keys";
        }

        public static class Telegram
        {
            private const string Base = $"{ApiBase}/telegram";
            public const string Webhook = $"{Base}/webhook";
            public const string GenerateToken = $"{Base}/link-token";
            public const string Status = $"{Base}/status";
            public const string Unlink = $"{Base}/unlink";
        }

        public static class Notifications
        {
            private const string Base = $"{ApiBase}/notifications";
            public const string GetAll = Base;
            public const string UnreadCount = $"{Base}/unread-count";
            public const string MarkRead = $"{Base}/{{notificationId:guid}}/read";
            public const string MarkAllRead = $"{Base}/read-all";
        }

        public static class PublicConfig
        {
            private const string ConfigBase = $"{ApiBase}/config";
            public const string Currencies = $"{ConfigBase}/currencies";
            public const string Navigation = $"{ConfigBase}/navigation";
            public const string Translations = $"{ConfigBase}/translations/{{language}}";
            public const string News = $"{ConfigBase}/news";
            public const string SeoConfig = $"{ConfigBase}/seo";
            public const string Languages = $"{ConfigBase}/languages";
        }
    }

    public static class Seo
    {
        public const string Sitemap = "/sitemap.xml";
        public const string Robots = "/robots.txt";
    }
}