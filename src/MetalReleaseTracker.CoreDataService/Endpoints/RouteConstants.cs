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
        }

        public static class Bands
        {
            private const string Base = $"{ApiBase}/bands";
            public const string GetAll = $"{Base}/all";
            public const string GetById = $"{Base}/{{id:guid}}";
            public const string GetWithAlbumCount = $"{Base}/with-album-count";
            public const string GetGenres = $"{Base}/genres";
            public const string GetSimilar = $"{Base}/{{id:guid}}/similar";
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
        }
    }
}