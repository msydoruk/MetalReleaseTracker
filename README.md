# Metal Release Tracker :ukraine:

Aggregator for physical releases (vinyl, CD, tape) of Ukrainian metal bands sold by foreign distributors and labels. Automatically scrapes distributor catalogs, detects new/updated/deleted albums, publishes changes through a Kafka event pipeline, and presents everything in a single searchable catalog with direct links to stores.

**Production**: https://metal-release.com

## Architecture

```
ParserService → Kafka (albums-processed-topic) → CoreDataService (API + React SPA)
```

### ParserService (.NET 10 Worker)

Scrapes album data from 9 distributor websites, detects catalog changes, and publishes events to Kafka.

- **Four-job pipeline**: BandReferenceSyncJob → CatalogueIndexJob → AlbumDetailParsingJob → AlbumPublisherJob
- **BandReferenceSyncJob** — syncs Ukrainian band list from Metal Archives
- **CatalogueIndexJob** — crawls distributor catalogs, builds index of all listings
- **AlbumDetailParsingJob** — parses album details (price, media type, cover art), detects changes
- **AlbumPublisherJob** — publishes new/updated/deleted albums to Kafka
- **AI Verification** — uses Claude API to match catalog items to Metal Archives discographies, resolving band name collisions
- Uploads album covers and band photos to MinIO
- Scraping: HtmlAgilityPack for most sites, Selenium WebDriver + FlareSolverr for anti-bot protected sites
- Stack: PostgreSQL (EF Core), TickerQ, Autofac DI, MassTransit Kafka Rider

### CoreDataService (.NET 10 API + SPA)

REST API (Minimal APIs) that serves the React frontend and consumes processed album events from Kafka.

- Consumes `albums-processed-topic` and `band-photos-synced-topic` via MassTransit
- Authentication: Google OAuth + email/password with JWT tokens
- YARP reverse proxy for MinIO storage (`/storage/` → MinIO) — no pre-signed URLs
- React SPA built into `wwwroot/` and served by Kestrel (no separate nginx container)
- Swagger UI at `/swagger`
- Stack: PostgreSQL (EF Core + ASP.NET Identity), MassTransit Kafka Rider

### Frontend (React 19 + Material-UI 7)

Single-page application bundled into the CoreDataService Docker image at build time.

- Pages: Albums (with filters, search, grouping), Bands, Distributors, News, About, Reviews, Changelog
- Features: favorites, full-size cover viewing, price comparison across stores, i18n (EN/UA)
- For local dev: `npm start` proxies API calls to `localhost:5002`

### SharedLibraries

Shared project with MinIO helpers referenced by both services.

### SharedInfrastructure

Docker Compose for shared dependencies: Kafka (KRaft mode), MinIO, OpenTelemetry Collector, Loki, Grafana.

## Quick Start

### Prerequisites

- .NET 10.0 SDK
- Docker & Docker Compose
- Node.js 18+ (for local frontend dev)

### 1. Shared Infrastructure (start first)

```bash
docker compose -f src/MetalReleaseTracker.SharedInfrastructure/docker-compose.yml up -d
```

### 2. Services

```bash
# ParserService
docker compose -f src/MetalReleaseTracker.ParserService/docker-compose.yml up -d --build

# CoreDataService (includes React frontend)
docker compose -f src/MetalReleaseTracker.CoreDataService/docker-compose.yml up -d --build
```

### Local Development (without Docker)

```bash
# Build
dotnet build src/MetalReleaseTracker.sln

# Run services (from repo root)
dotnet run --project src/MetalReleaseTracker.ParserService
dotnet run --project src/MetalReleaseTracker.CoreDataService

# Frontend (dev mode, proxies to localhost:5002)
cd src/MetalReleaseTracker.Frontend && npm install --legacy-peer-deps && npm start
```

## Ports

### Services

| Port | Service |
|------|---------|
| 5000 | ParserService (API + TickerQ dashboard) |
| 5002 | CoreDataService (API + React SPA) |
| 8191 | FlareSolverr (anti-bot proxy) |

### Databases

| Port | Service | Database |
|------|---------|----------|
| 5434 | PostgreSQL | ParserServiceDb |
| 5436 | PostgreSQL | CoreDataServiceDb |

### Infrastructure

| Port | Service |
|------|---------|
| 3000 | Grafana (logs) |
| 9001 | MinIO API |
| 9002 | MinIO Console |
| 9092/9093 | Kafka (internal/external) |
| 4317/4318 | OTel Collector (gRPC/HTTP) |

## Dashboards

| Dashboard | URL |
|-----------|-----|
| CoreDataService (SPA) | http://localhost:5002 |
| Swagger | http://localhost:5002/swagger |
| TickerQ | http://localhost:5000/tickerq/dashboard |
| Grafana | http://localhost:3000 |
| MinIO Console | http://localhost:9002 |

## Development

### Build & Test

```bash
dotnet build src/MetalReleaseTracker.sln
dotnet test src/MetalReleaseTracker.ParserService
dotnet test src/MetalReleaseTracker.CoreDataService
```

Tests use xUnit + Moq + Testcontainers (requires Docker running). Tests are embedded in each service under `Tests/` folder.

### Database Migrations

```bash
# ParserService
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.ParserService

# CoreDataService
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context CoreDataServiceDbContext
dotnet ef migrations add <Name> --project src/MetalReleaseTracker.CoreDataService --context IdentityServerDbContext
```

## Deploy

GitHub Actions workflow (`.github/workflows/deploy.yml`) — manual dispatch:
- `all` | `shared-infrastructure` | `parser-service` | `core-data-service`

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `SSH_KEY` | SSH private key |
| `SSH_USER` | SSH user (`ubuntu`) |
| `SSH_HOST` | Server IP |
| `ENV_SHARED_INFRA` | SharedInfrastructure .env |
| `ENV_PARSER_SERVICE` | ParserService .env |
| `ENV_CORE_DATA` | CoreDataService .env |

### SSL Certificate

- **Provider**: Let's Encrypt (certbot)
- **Domain**: metal-release.com, www.metal-release.com
- **Auto-renewal**: certbot timer (systemd)
- **Nginx reverse proxy**: `/etc/nginx/sites-enabled/metaltracker` → localhost:5002
- **Renewal check**: `sudo certbot renew --dry-run`

## Technology Stack

| Category | Technologies |
|----------|-------------|
| Runtime | .NET 10.0 |
| API | ASP.NET Core Minimal APIs |
| Database | PostgreSQL 15, EF Core, ASP.NET Identity |
| Messaging | Apache Kafka (KRaft), MassTransit Kafka Rider |
| Scheduling | TickerQ |
| Object Storage | MinIO (S3-compatible), YARP reverse proxy |
| Observability | OpenTelemetry, Grafana, Loki |
| Scraping | HtmlAgilityPack, Selenium WebDriver, FlareSolverr |
| AI | Claude API (album verification) |
| DI | Autofac (ParserService), built-in (CoreDataService) |
| Frontend | React 19, Material-UI 7 |
| Testing | xUnit, Moq, Testcontainers |

## Project Structure

```
src/
├── MetalReleaseTracker.ParserService/       # Scraper + change detection + Kafka producer
├── MetalReleaseTracker.CoreDataService/     # REST API + SPA host + Kafka consumer
├── MetalReleaseTracker.Frontend/            # React SPA (built into CoreDataService wwwroot/)
├── MetalReleaseTracker.SharedInfrastructure/ # Kafka, MinIO, OTel, Loki, Grafana
└── MetalReleaseTracker.SharedLibraries/     # Shared MinIO helpers
```
