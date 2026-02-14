# Metal Release Tracker

Event-driven microservices system for tracking metal album releases. Scrapes distributor websites, processes data through Kafka pipeline, and provides REST API for frontend.

## Architecture

```
ParserService → Kafka → CatalogSyncService → Kafka → CoreDataService → Frontend
```

### Services

**ParserService** (Worker)
- Scrapes album data from distributor websites
- Uploads album covers to MinIO
- Publishes to `albums-parsed-topic` via transactional outbox
- Stack: PostgreSQL (EF Core), TickerQ, Kafka (MassTransit)

**CatalogSyncService** (Worker)
- Validates and transforms raw album data
- Detects changes (new/updated/deleted albums)
- Publishes to `albums-processed-topic`
- Stack: MongoDB (30-day TTL), TickerQ, PostgreSQL (TickerQ only), Kafka (MassTransit)

**CoreDataService** (API)
- REST API for frontend
- Authentication (Google OAuth + JWT)
- Generates pre-signed MinIO URLs for images
- Stack: PostgreSQL (EF Core), ASP.NET Core, Kafka consumer

**Frontend** (React)
- React 19 + Material-UI 7
- Nginx container with API proxy to CoreDataService

## Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker & Docker Compose
- Node.js 18+ (for local frontend dev)

### 1. Shared Infrastructure (start first)

```bash
cd src/MetalReleaseTracker.SharedInfrastructure
docker compose up -d
```

### 2. Services

```bash
# ParserService
cd src/MetalReleaseTracker.ParserService
docker compose up -d --build

# CatalogSyncService
cd src/MetalReleaseTracker.CatalogSyncService
docker compose up -d --build

# CoreDataService
cd src/MetalReleaseTracker.CoreDataService
docker compose up -d --build

# Frontend
cd src/MetalReleaseTracker.Frontend
docker compose up -d --build
```

### Local Development (without Docker)

```bash
cd src
dotnet build MetalReleaseTracker.sln

dotnet run --project MetalReleaseTracker.ParserService
dotnet run --project MetalReleaseTracker.CatalogSyncService
dotnet run --project MetalReleaseTracker.CoreDataService

cd MetalReleaseTracker.Frontend && npm install --legacy-peer-deps && npm start
```

## Ports

### Services

| Port | Service |
|------|---------|
| 3001 | Frontend (nginx) |
| 5000 | ParserService |
| 5001 | CatalogSyncService |
| 5002 | CoreDataService API |

### Databases

| Port | Service | Database |
|------|---------|----------|
| 5434 | PostgreSQL | ParserServiceDb (data + TickerQ) |
| 5435 | PostgreSQL | TickerQDb_CatalogSync |
| 5436 | PostgreSQL | core_data_service_db |
| 27017 | MongoDB | CatalogSyncServiceDb |

### Infrastructure

| Port | Service |
|------|---------|
| 3000 | Grafana (logs, metrics, traces) |
| 9001 | MinIO API |
| 9002 | MinIO Console |
| 9003 | Kafdrop (Kafka UI) |
| 9092/9093 | Kafka (internal/external) |
| 4317/4318 | OTel Collector (gRPC/HTTP) |

## Dashboards

| Dashboard | URL |
|-----------|-----|
| Frontend | http://localhost:3001 |
| Grafana | http://localhost:3000 |
| Parser TickerQ | http://localhost:5000/tickerq/dashboard |
| CatalogSync TickerQ | http://localhost:5001/tickerq/dashboard |
| MinIO Console | http://localhost:9002 |
| Kafdrop | http://localhost:9003 |

## Development

### Build & Test

```bash
dotnet build MetalReleaseTracker.sln
dotnet test MetalReleaseTracker.sln
dotnet test --filter "FullyQualifiedName~AlbumParsingJobTests"
```

### Database Migrations

```bash
# ParserService
cd MetalReleaseTracker.ParserService
dotnet ef migrations add <Name>

# CoreDataService
cd MetalReleaseTracker.CoreDataService
dotnet ef migrations add <Name> --context CatalogDataServiceDbContext
dotnet ef migrations add <Name> --context IdentityServerDbContext
```

## Deploy

GitHub Actions workflow (`.github/workflows/deploy.yml`) — manual dispatch:
- `all` | `shared-infrastructure` | `parser-service` | `catalog-sync-service` | `core-data-service` | `frontend`

### Required GitHub Secrets

| Secret | Description |
|--------|-------------|
| `SSH_KEY` | SSH private key |
| `SSH_USER` | SSH user (`ubuntu`) |
| `SSH_HOST` | Server IP |
| `ENV_SHARED_INFRA` | SharedInfrastructure .env |
| `ENV_PARSER_SERVICE` | ParserService .env |
| `ENV_CATALOG_SYNC` | CatalogSyncService .env |
| `ENV_CORE_DATA` | CoreDataService .env |

## Technology Stack

| Category | Technologies |
|----------|-------------|
| Runtime | .NET 10.0 |
| API | ASP.NET Core |
| Database | PostgreSQL (EF Core), MongoDB |
| Messaging | Kafka (MassTransit 8.3) |
| Scheduling | TickerQ 10.1.1 |
| Storage | MinIO (S3-compatible) |
| Observability | OpenTelemetry, Grafana, Tempo, Loki, Prometheus |
| Scraping | HtmlAgilityPack |
| Validation | FluentValidation |
| Frontend | React 19, Material-UI 7, nginx |
| Testing | xUnit, Testcontainers |

## Project Structure

```
src/
├── MetalReleaseTracker.ParserService/       # Album scraper worker
├── MetalReleaseTracker.CatalogSyncService/  # Data validation worker
├── MetalReleaseTracker.CoreDataService/     # REST API
├── MetalReleaseTracker.Frontend/            # React UI (nginx)
├── MetalReleaseTracker.SharedInfrastructure/ # Kafka, MinIO, OTel, Grafana
├── MetalReleaseTracker.SharedLibraries/     # Shared utilities
└── MetalReleaseTracker.Benchmarks/          # Performance tests
```
