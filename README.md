# Metal Release Tracker

Event-driven microservices system for tracking metal album releases. Scrapes distributor websites, processes data through Kafka pipeline, and provides REST API for frontend.

## Architecture

```
ParserService → Kafka → CatalogSyncService → Kafka → CoreDataService → Frontend
```

### Services

**ParserService** (Worker)
- Scrapes album data from distributor websites
- Uploads album covers to MinIO object storage
- Publishes to `albums-parsed-topic` via transactional outbox pattern
- Stack: PostgreSQL (EF Core), TickerQ (scheduling), Kafka (MassTransit)

**CatalogSyncService** (Worker)
- Validates and transforms raw album data
- Detects changes (new/updated/deleted albums)
- Publishes to `albums-processed-topic`
- Stack: MongoDB (30-day TTL), Hangfire (scheduling), Kafka (MassTransit)

**CoreDataService** (API)
- REST API for frontend
- Authentication (Google OAuth + JWT)
- Generates pre-signed MinIO URLs for images
- Stack: PostgreSQL (EF Core), ASP.NET Core, Kafka consumer

**Frontend**
- React + Material-UI
- OAuth authentication via oidc-client-ts

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker & Docker Compose
- Node.js 18+ (for frontend)

### Run Infrastructure

```bash
# ParserService (Kafka, PostgreSQL, MinIO, Seq)
cd src/MetalReleaseTracker.ParserService
docker-compose up -d

# CatalogSyncService (MongoDB, Hangfire PostgreSQL)
cd src/MetalReleaseTracker.CatalogSyncService
docker-compose up -d

# CoreDataService (PostgreSQL)
cd src/MetalReleaseTracker.CoreDataService
docker-compose up -d
```

### Run Services

```bash
# From src/ directory
dotnet build MetalReleaseTracker.sln

# Terminal 1 - ParserService
dotnet run --project MetalReleaseTracker.ParserService

# Terminal 2 - CatalogSyncService
dotnet run --project MetalReleaseTracker.CatalogSyncService

# Terminal 3 - CoreDataService
dotnet run --project MetalReleaseTracker.CoreDataService

# Terminal 4 - Frontend
cd MetalReleaseTracker.Frontend
npm install
npm start
```

### Infrastructure URLs

| Service | URL | Credentials |
|---------|-----|-------------|
| Kafdrop (Kafka UI) | http://localhost:9003 | - |
| Seq (Logs) | http://localhost:5341 | - |
| MinIO Console | http://localhost:9002 | admin / S3cur3P@ssw0rd! |
| TickerQ Dashboard | http://localhost:5000 | - |
| Hangfire Dashboard | http://localhost:5002 | - |
| API Swagger | http://localhost:5001/swagger | - |
| Frontend | http://localhost:3000 | - |

## Development

### Build & Test

```bash
# Build solution
dotnet build MetalReleaseTracker.sln

# Run all tests
dotnet test MetalReleaseTracker.sln

# Run specific test
dotnet test --filter "FullyQualifiedName~AlbumParsingJobTests"

# Run benchmarks
dotnet run --project MetalReleaseTracker.Benchmarks -c Release
```

### Database Migrations

```bash
# ParserService
cd MetalReleaseTracker.ParserService
dotnet ef migrations add <MigrationName>
dotnet ef database update

# CoreDataService (CatalogDataServiceDbContext)
cd MetalReleaseTracker.CoreDataService
dotnet ef migrations add <MigrationName> --context CatalogDataServiceDbContext
dotnet ef database update --context CatalogDataServiceDbContext

# CoreDataService (IdentityServerDbContext)
dotnet ef migrations add <MigrationName> --context IdentityServerDbContext
dotnet ef database update --context IdentityServerDbContext
```

### Adding New Parser

1. Implement `IParser` interface in `MetalReleaseTracker.ParserService/Infrastructure/Parsers/`
2. Register in `ParserRegistrationExtension.cs`:
```csharp
builder.RegisterType<YourParser>()
    .As<IParser>()
    .WithMetadata<ParserMetadata>(m =>
        m.For(meta => meta.DistributorCode, DistributorCode.YourDistributor));
```
3. Add data source in `appsettings.json`:
```json
"ParserDataSources": [{
  "DistributorCode": "YourDistributor",
  "Name": "yourdistributor.com",
  "ParsingUrl": "https://yourdistributor.com/releases"
}]
```

## Technology Stack

| Category | Technologies |
|----------|-------------|
| Runtime | .NET 8.0 |
| API | ASP.NET Core Minimal APIs |
| Workers | .NET Generic Host |
| Database | PostgreSQL (EF Core 9), MongoDB |
| Messaging | Kafka (MassTransit 8.3) |
| Scheduling | TickerQ, Hangfire |
| Storage | MinIO (S3-compatible) |
| Logging | Serilog + Seq |
| Scraping | HtmlAgilityPack |
| Validation | FluentValidation |
| DI | Autofac + Microsoft.Extensions.DI |
| Testing | xUnit + Testcontainers |
| Frontend | React 19, Material-UI 7, React Router 7 |

## Key Patterns

**Outbox Pattern** - ParserService ensures reliable Kafka publishing via transactional outbox
**Large Payloads** - Albums stored in MinIO, Kafka messages contain file paths only
**Repository Pattern** - All database access abstracted behind interfaces
**Event-Driven** - Services communicate asynchronously via Kafka topics
**TTL Strategy** - MongoDB documents expire after 30 days (temporary storage)

## Project Structure

```
src/
├── MetalReleaseTracker.ParserService/      # Album scraper worker
│   ├── Infrastructure/Parsers/             # Distributor-specific parsers
│   ├── Infrastructure/Jobs/                # Background jobs
│   └── Tests/                              # Integration tests
├── MetalReleaseTracker.CatalogSyncService/ # Data validation worker
│   ├── Data/Validators/                    # FluentValidation rules
│   └── Tests/                              # Integration tests
├── MetalReleaseTracker.CoreDataService/    # REST API
│   ├── Endpoints/                          # Minimal API endpoints
│   ├── Consumers/                          # Kafka consumers
│   └── Tests/                              # Integration tests
├── MetalReleaseTracker.Frontend/           # React UI
├── MetalReleaseTracker.SharedLibraries/    # Shared utilities
└── MetalReleaseTracker.Benchmarks/         # Performance tests
```

## Monitoring

**Kafka** - Monitor topics and consumer lag via Kafdrop UI
**Jobs** - Check TickerQ/Hangfire dashboards for job status and failures
**Logs** - Structured logging in Seq with correlation IDs
**Storage** - Browse MinIO console for images and large payloads

## Configuration

Environment-specific settings in `appsettings.json`:
- Kafka bootstrap servers and topics
- Database connection strings
- MinIO endpoint and credentials
- Serilog + Seq configuration
- Job schedules (cron expressions)
- Parser data sources

Sensitive data (OAuth secrets, production passwords) stored in user secrets.
