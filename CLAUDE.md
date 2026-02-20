# CLAUDE.md

## Build & Run Commands

```bash
# Build entire solution
dotnet build src/MetalReleaseTracker.sln

# Run individual services (from repo root)
dotnet run --project src/MetalReleaseTracker.ParserService
dotnet run --project src/MetalReleaseTracker.CatalogSyncService
dotnet run --project src/MetalReleaseTracker.CoreDataService

# Frontend
cd src/MetalReleaseTracker.Frontend && npm install --legacy-peer-deps && npm start

# Docker: start shared infra first, then services
docker compose -f src/MetalReleaseTracker.SharedInfrastructure/docker-compose.yml up -d
docker compose -f src/MetalReleaseTracker.ParserService/docker-compose.yml up -d --build
docker compose -f src/MetalReleaseTracker.CatalogSyncService/docker-compose.yml up -d --build
docker compose -f src/MetalReleaseTracker.CoreDataService/docker-compose.yml up -d --build
docker compose -f src/MetalReleaseTracker.Frontend/docker-compose.yml up -d --build

# Tests (xUnit with Testcontainers - requires Docker running)
dotnet test src/MetalReleaseTracker.ParserService
dotnet test src/MetalReleaseTracker.CatalogSyncService
dotnet test src/MetalReleaseTracker.CoreDataService

# EF Core migrations
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.ParserService
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context CoreDataServiceDbContext
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context IdentityServerDbContext
```

## Architecture

Event-driven pipeline for tracking Ukrainian metal band releases:

```
ParserService -> Kafka (albums-parsed-topic) -> CatalogSyncService -> Kafka (albums-processed-topic) -> CoreDataService -> Frontend
```

**ParserService** (.NET 10 Worker) - Scrapes album data from 3 distributor websites using HtmlAgilityPack and Selenium WebDriver. Uploads cover images to MinIO. Publishes parsed events via transactional outbox pattern. PostgreSQL (`ParserServiceDb`, port 5434).

**CatalogSyncService** (.NET 10 Worker) - Consumes parsed events, validates with FluentValidation, detects new/updated/deleted albums. MongoDB for raw album storage (30-day TTL). PostgreSQL for TickerQ scheduling only (port 5435).

**CoreDataService** (.NET 10 API) - REST API with Minimal APIs. Consumes processed events, serves albums/bands/distributors to frontend. Google OAuth + JWT auth. PostgreSQL (`CoreDataServiceDb`, port 5436). Swagger at `/swagger`.

**Frontend** (React 19 + Material-UI 7) - SPA proxied to CoreDataService at `localhost:5002`. Nginx in Docker.

**SharedLibraries** - Shared project referenced by services.

**SharedInfrastructure** - Docker Compose for Kafka, Zookeeper, MinIO, Kafdrop, OpenTelemetry Collector, Grafana, Tempo, Loki, Prometheus.

## Key Patterns

- **Minimal APIs** - CoreDataService uses `MapGet`/`MapPost` endpoint mapping (see `Endpoints/` folder)
- **Repository + Service layers** - Each service has `Repositories/` and `Services/` with interface-based DI
- **Autofac DI** - ParserService and CatalogSyncService replace default DI with `AutofacServiceProviderFactory`
- **MassTransit + Kafka** - Dual transport (InMemory + Kafka Rider). Producers use `ITopicProducer<T>`, consumers implement `IConsumer<T>`
- **TickerQ** - Scheduled jobs in ParserService and CatalogSyncService. Dashboard at `/tickerq/dashboard`
- **FluentValidation** - `AbstractValidator<T>` in CatalogSyncService for raw album validation
- **Transactional outbox** - ParserService persists events to DB before publishing to Kafka
- **MinIO** - Album cover storage with pre-signed URLs for frontend access
- **Service registration extensions** - `*RegistrationExtension.cs` / `*Extension.cs` static classes with `IServiceCollection` extension methods
- **OpenTelemetry** - Traces (Tempo), logs (Loki), metrics (Prometheus), dashboards (Grafana)
- **Tests** - Embedded in each service under `Tests/` folder (not separate projects). xUnit + Moq + Testcontainers

## Code Style

- **StyleCop.Analyzers** enforced via `.editorconfig` at repo root with **error** severity
- Key rules: spacing (SA1000-SA1028), ordering (SA1203-SA1217), naming (SA1302-SA1314), maintainability (SA1400-SA1411), layout (SA1500-SA1518)
- Documentation rules (SA16xx) are set to **none** (not enforced)
- Nullable reference types enabled, implicit usings enabled
- All services target .NET 10 (`net10.0`)
- **No short parameter names**: Use full descriptive names - `cancellationToken` (not `ct`), `exception` (not `ex`), etc.
- **Class member ordering**: Fields, constructor, public methods, private instance methods, private static methods (static helpers go at the bottom)

## Workflow Rules

- **Confirm before implementing**: Before making ANY code changes, explain the approach and reasoning. Wait for explicit user approval before writing/editing files. If there are multiple ways to solve a problem, present the options and let the user choose.
- **No AI attribution in commits**: Never include `Co-Authored-By` or any other mention of AI/Claude/assistant in commit messages.
- **Confirm before pushing**: Before `git push`, always show the user the full list of files being pushed and a summary of changes. Ask for explicit confirmation. Do not push one-time scripts, temporary files, screenshots, or other artifacts that don't belong in the repository.
- **Confirm before DB changes**: Before executing any SQL scripts against the production database, always show the user the full SQL script and ask for explicit confirmation. Never run UPDATE/DELETE/INSERT queries on production without prior approval.
- **Split merge requests by scope**: When changes span unrelated areas (e.g., TickerQ refactoring + frontend UI + parser logic), create separate branches and PRs for each logical group. Never mix unrelated changes into a single PR.
- **Deploy via GitHub Actions only**: Never deploy directly to the server instance via SSH. All deployments go through GitHub Actions workflow (`.github/workflows/deploy.yml`). Push to `main` triggers the deployment pipeline automatically.
