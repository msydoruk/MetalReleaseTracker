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

    public static class CatalogueDetails
    {
        private const string Prefix = $"{Base}/catalogue-details";
        public const string GetAll = Prefix;
    }

    public static class AiVerification
    {
        private const string Prefix = $"{Base}/ai-verification";
        public const string GetAll = Prefix;
        public const string Run = $"{Prefix}/run";
        public const string SetDecision = $"{Prefix}/{{id:guid}}/decision";
        public const string BatchDecision = $"{Prefix}/batch-decision";
        public const string BulkDecision = $"{Prefix}/bulk-decision";
        public const string ResetForReanalysis = $"{Prefix}/reset-for-reanalysis";
    }

    public static class ParsingMonitor
    {
        private const string Prefix = $"{Base}/parsing-monitor";
        public const string Live = $"{Prefix}/live";
        public const string Runs = $"{Prefix}/runs";
        public const string RunById = $"{Prefix}/runs/{{runId:guid}}";
        public const string RunItems = $"{Prefix}/runs/{{runId:guid}}/items";
    }

    public static class BandPhotos
    {
        private const string Prefix = $"{Base}/band-photos";
        public const string Sync = $"{Prefix}/sync";
    }

    public static class Settings
    {
        private const string Prefix = $"{Base}/settings";

        public static class AiAgents
        {
            private const string AgentPrefix = $"{Prefix}/ai-agents";
            public const string GetAll = AgentPrefix;
            public const string GetById = $"{AgentPrefix}/{{id:guid}}";
            public const string Create = AgentPrefix;
            public const string Update = $"{AgentPrefix}/{{id:guid}}";
            public const string Delete = $"{AgentPrefix}/{{id:guid}}";
        }

        public static class ParsingSources
        {
            private const string SourcePrefix = $"{Prefix}/parsing-sources";
            public const string GetAll = SourcePrefix;
            public const string Update = $"{SourcePrefix}/{{id:guid}}";
        }

        public const string BandReference = $"{Prefix}/band-reference";
        public const string FlareSolverr = $"{Prefix}/flaresolverr";
        public const string GeneralParser = $"{Prefix}/general-parser";
    }
}
