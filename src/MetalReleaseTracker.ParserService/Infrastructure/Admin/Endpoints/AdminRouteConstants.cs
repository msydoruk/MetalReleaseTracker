namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class AdminRouteConstants
{
    private const string Base = "api/admin";

    public static class Auth
    {
        public const string Login = $"{Base}/auth/login";
    }

    public static class BandReferences
    {
        private const string Prefix = $"{Base}/band-references";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
    }

    public static class CatalogueIndex
    {
        private const string Prefix = $"{Base}/catalogue-index";
        public const string GetAll = Prefix;
        public const string UpdateStatus = $"{Prefix}/{{id:guid}}/status";
        public const string BatchUpdateStatus = $"{Prefix}/batch-status";
    }

    public static class ParsingSessions
    {
        private const string Prefix = $"{Base}/parsing-sessions";
        public const string GetAll = Prefix;
        public const string GetById = $"{Prefix}/{{id:guid}}";
        public const string UpdateStatus = $"{Prefix}/{{id:guid}}/status";
    }

    public static class AiVerification
    {
        private const string Prefix = $"{Base}/ai-verification";
        public const string GetAll = Prefix;
        public const string Run = $"{Prefix}/run";
        public const string SetDecision = $"{Prefix}/{{id:guid}}/decision";
        public const string BatchDecision = $"{Prefix}/batch-decision";
        public const string BulkDecision = $"{Prefix}/bulk-decision";
    }
}
