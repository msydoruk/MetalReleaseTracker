# CLAUDE.md

## Build & Run Commands

```bash
# Build entire solution
dotnet build src/MetalReleaseTracker.sln

# Run individual services (from repo root)
dotnet run --project src/MetalReleaseTracker.ParserService
dotnet run --project src/MetalReleaseTracker.CoreDataService

# Frontend (dev mode, proxies to CoreDataService at localhost:5002)
cd src/MetalReleaseTracker.Frontend && npm install --legacy-peer-deps && npm start

# Docker: start shared infra first, then services
docker compose -f src/MetalReleaseTracker.SharedInfrastructure/docker-compose.yml up -d
docker compose -f src/MetalReleaseTracker.ParserService/docker-compose.yml up -d --build
docker compose -f src/MetalReleaseTracker.CoreDataService/docker-compose.yml up -d --build

# Tests (xUnit with Testcontainers - requires Docker running)
dotnet test src/MetalReleaseTracker.ParserService
dotnet test src/MetalReleaseTracker.CoreDataService

# EF Core migrations
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.ParserService
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context CoreDataServiceDbContext
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context IdentityServerDbContext
```

## Database Access

Connect to service databases via Docker. Credentials and ports are in each service's `.env` and `docker-compose.yml`.

```bash
# Connect via Docker (credentials in each service's .env file)
docker exec -i <postgres-container> env PGPASSWORD='<from .env>' psql -U <user> -d <dbname> -c "<SQL>"

# Example: query ParserServiceDb
docker exec -i metalrelease_postgres_parser env PGPASSWORD='...' psql -U parser_admin -d ParserServiceDb -c 'SELECT COUNT(*) FROM "CatalogueIndex";'
```

Service database ports: ParserService=5434, CoreDataService=5436. PostgreSQL column names are PascalCase and must be double-quoted in SQL.

**ParserServiceDb** (PostgreSQL, port 5434): BandReferences, BandDiscography, CatalogueIndex, CatalogueIndexDetails, AiVerifications, AiAgents, ParsingSources, Settings.

**CoreDataServiceDb** (PostgreSQL, port 5436): Albums, Bands, Distributors, Feedbacks, RefreshTokens, UserFavorites + ASP.NET Identity tables.

Full schema with columns, types, and FK relationships: [`docs/database-schema.md`](docs/database-schema.md).

## Architecture

Event-driven pipeline for tracking Ukrainian metal band releases:

```
ParserService -> Kafka (albums-processed-topic) -> CoreDataService (API + SPA)
```

**ParserService** (.NET 10 Worker) - Scrapes album data from distributor websites using HtmlAgilityPack and Selenium WebDriver. Uploads cover images to MinIO. Detects new/updated/deleted albums via CatalogueIndexDetails and publishes directly to Kafka. PostgreSQL (`ParserServiceDb`, port 5434).

**CoreDataService** (.NET 10 API + React SPA) - REST API with Minimal APIs + serves React frontend from `wwwroot/`. Consumes processed events, serves albums/bands/distributors. YARP reverse proxy for MinIO storage (`/storage/` -> MinIO). Google OAuth + JWT auth. PostgreSQL (`CoreDataServiceDb`, port 5436). Swagger at `/swagger`.

**Frontend** (React 19 + Material-UI 7) - Source in `src/MetalReleaseTracker.Frontend/`, built and bundled into CoreDataService Docker image. For local dev, run `npm start` which proxies to `localhost:5002`.

**SharedLibraries** - Shared project referenced by services.

**SharedInfrastructure** - Docker Compose for Kafka, Zookeeper, MinIO, Kafdrop, OpenTelemetry Collector, Grafana, Loki.

## Key Patterns

- **Minimal APIs** - CoreDataService uses `MapGet`/`MapPost` endpoint mapping (see `Endpoints/` folder)
- **Repository + Service layers** - Each service has `Repositories/` and `Services/` with interface-based DI
- **Autofac DI** - ParserService replaces default DI with `AutofacServiceProviderFactory`
- **MassTransit + Kafka** - Dual transport (InMemory + Kafka Rider). Producers use `ITopicProducer<T>`, consumers implement `IConsumer<T>`
- **TickerQ** - Scheduled jobs in ParserService. Dashboard at `/tickerq/dashboard`
- **MinIO** - Album cover storage with pre-signed URLs for frontend access
- **Service registration extensions** - `*RegistrationExtension.cs` / `*Extension.cs` static classes with `IServiceCollection` extension methods
- **YARP** - Reverse proxy in CoreDataService for MinIO storage URLs (`/storage/` -> MinIO)
- **OpenTelemetry** - Logs (Loki), dashboards (Grafana). Services send OTLP telemetry; OTEL Collector forwards logs only
- **Tests** - Embedded in each service under `Tests/` folder (not separate projects). xUnit + Moq + Testcontainers

## Code Style

- **StyleCop.Analyzers** enforced via `.editorconfig` at repo root with **error** severity
- Key rules: spacing (SA1000-SA1028), ordering (SA1203-SA1217), naming (SA1302-SA1314), maintainability (SA1400-SA1411), layout (SA1500-SA1518)
- Documentation rules (SA16xx) are set to **none** (not enforced)
- Nullable reference types enabled, implicit usings enabled
- All services target .NET 10 (`net10.0`)
- **No short parameter names**: Use full descriptive names - `cancellationToken` (not `ct`), `exception` (not `ex`), etc.
- **Class member ordering**: Fields, constructor, public methods, private instance methods, private static methods (static helpers go at the bottom)

## Artifacts

All temporary and generated files (screenshots, one-off scripts, drafts, test reports, etc.) MUST be saved to the `.artifacts/` folder in the repo root. Never create such files in the repo root or inside `src/`. The `.artifacts/` folder is git-ignored.

## Workflow Rules

- **Plan before substantial changes**: All non-trivial changes (multi-file edits, new features, refactoring, bug investigations) MUST start with a plan. Use plan mode to explore the codebase, draft a step-by-step approach, and get explicit user approval before writing any code.
- **Confirm before implementing**: Before making ANY code changes, explain the approach and reasoning. Wait for explicit user approval before writing/editing files. If there are multiple ways to solve a problem, present the options and let the user choose.
- **No AI attribution in commits**: Never include `Co-Authored-By` or any other mention of AI/Claude/assistant in commit messages.
- **Confirm before pushing**: Before `git push`, always show the user the full list of files being pushed and a summary of changes. Ask for explicit confirmation. Do not push one-time scripts, temporary files, screenshots, or other artifacts that don't belong in the repository.
- **Confirm before DB changes**: Before executing any SQL scripts against the production database, always show the user the full SQL script and ask for explicit confirmation. Never run UPDATE/DELETE/INSERT queries on production without prior approval.
- **Split merge requests by scope**: When changes span unrelated areas (e.g., TickerQ refactoring + frontend UI + parser logic), create separate branches and PRs for each logical group. Never mix unrelated changes into a single PR.
- **Deploy via GitHub Actions only**: Never deploy directly to the server instance via SSH. All deployments go through GitHub Actions workflow (`.github/workflows/deploy.yml`). Push to `main` triggers the deployment pipeline automatically.
- **Mandatory UI testing**: For any frontend UI changes, build Docker locally (`docker compose -f src/MetalReleaseTracker.CoreDataService/docker-compose.yml up -d --build`), then verify changes with Playwright browser tools on both desktop and mobile (375x812) viewports.
