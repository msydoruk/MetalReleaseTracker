# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Metal Release Tracker is a microservices-based system for scraping, processing, and serving metal album release data. The architecture follows an event-driven pattern with three main services communicating via Kafka.

**Data Flow**: ParserService → Kafka → CatalogSyncService → Kafka → CoreDataService

## Build & Run Commands

### Building the Solution
```bash
# Build entire solution (from src/ directory)
dotnet build MetalReleaseTracker.sln

# Build specific service
dotnet build MetalReleaseTracker.ParserService/MetalReleaseTracker.ParserService.csproj
dotnet build MetalReleaseTracker.CatalogSyncService/MetalReleaseTracker.CatalogSyncService.csproj
dotnet build MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj
```

### Running Services
```bash
# Run a service (from src/ directory)
dotnet run --project MetalReleaseTracker.ParserService
dotnet run --project MetalReleaseTracker.CatalogSyncService
dotnet run --project MetalReleaseTracker.CoreDataService
```

### Testing
```bash
# Run all tests in solution
dotnet test MetalReleaseTracker.sln

# Run tests for specific service
dotnet test MetalReleaseTracker.ParserService/MetalReleaseTracker.ParserService.csproj
dotnet test MetalReleaseTracker.CatalogSyncService/MetalReleaseTracker.CatalogSyncService.csproj
dotnet test MetalReleaseTracker.CoreDataService/MetalReleaseTracker.CoreDataService.csproj

# Run benchmarks
dotnet run --project MetalReleaseTracker.Benchmarks -c Release
```

### Database Migrations (EF Core)
```bash
# ParserService migrations
cd MetalReleaseTracker.ParserService
dotnet ef migrations add <MigrationName>
dotnet ef database update

# CoreDataService migrations (CatalogDataServiceDbContext)
cd MetalReleaseTracker.CoreDataService
dotnet ef migrations add <MigrationName> --context CatalogDataServiceDbContext
dotnet ef database update --context CatalogDataServiceDbContext

# CoreDataService migrations (IdentityServerDbContext)
dotnet ef migrations add <MigrationName> --context IdentityServerDbContext
dotnet ef database update --context IdentityServerDbContext
```

### Infrastructure (Docker Compose)

**Shared Infrastructure** (Required - Start First)
```bash
# Start shared infrastructure (Kafka, Seq, MinIO)
cd MetalReleaseTracker.SharedInfrastructure
docker-compose up -d
```

**Service-Specific Infrastructure** (Start After Shared Infrastructure)
```bash
# Start infrastructure for ParserService (PostgreSQL, TickerQ)
cd MetalReleaseTracker.ParserService
docker-compose up -d

# Start infrastructure for CatalogSyncService (MongoDB, Hangfire PostgreSQL)
cd MetalReleaseTracker.CatalogSyncService
docker-compose up -d

# Start infrastructure for CoreDataService (PostgreSQL)
cd MetalReleaseTracker.CoreDataService
docker-compose up -d
```

**Infrastructure Ports:**

*Shared Infrastructure:*
- Kafka: 9093 (external), 9092 (internal)
- Kafdrop (Kafka UI): http://localhost:9003
- Seq (Logging): http://localhost:5341
- MinIO: 9001 (API), 9002 (Console) - http://localhost:9002
- Zookeeper: 2181

*Service-Specific:*
- PostgreSQL (ParserService): 5434
- PostgreSQL (CatalogSyncService Hangfire): 5435
- PostgreSQL (CoreDataService): 5436
- MongoDB: 27017

## Architecture

### Service Responsibilities

**ParserService** (Worker Service)
- Web scrapes album data from distributor websites (currently OsmoseProductions)
- Downloads album cover images and stores them in MinIO
- Implements transactional outbox pattern for reliable Kafka publishing
- Uses Hangfire for scheduled daily parsing jobs
- **Database**: PostgreSQL (EF Core) - stores parsing sessions and outbox events
- **Publishes**: `AlbumParsedPublicationEvent` → `albums-parsed-topic`

**CatalogSyncService** (Worker Service)
- Consumes raw album data from ParserService
- Validates data using FluentValidation
- Detects changes (new/updated/deleted albums)
- Acts as data transformation layer between raw and processed data
- **Database**: MongoDB - temporary storage with 30-day TTL for raw albums
- **Consumes**: `albums-parsed-topic`
- **Publishes**: `AlbumProcessedPublicationEvent` → `albums-processed-topic`

**CoreDataService** (ASP.NET Core API)
- Main backend API for frontend application
- Provides REST endpoints for album catalog, bands, distributors
- Handles user authentication (Google OAuth + JWT)
- Generates pre-signed MinIO URLs for album images
- **Database**: PostgreSQL (EF Core) - normalized relational model
- **Consumes**: `albums-processed-topic`

**SharedLibraries**
- Reusable MinIO file storage abstraction (`IFileStorageService`)
- Content type helpers and common utilities

### Key Architectural Patterns

**Event-Driven Architecture**
- Services communicate asynchronously via Kafka topics
- Each service can scale independently
- Loose coupling between services

**Outbox Pattern** (ParserService)
- Parsed albums saved to database first (transactional)
- Separate job publishes from outbox to Kafka
- Ensures no data loss if Kafka is unavailable

**Large Payload Handling**
- Album data serialized to JSON and uploaded to MinIO in chunks (<1MB)
- Kafka messages contain MinIO file paths, not album data directly
- Avoids Kafka message size limitations

**Repository Pattern**
- All database access through repository interfaces
- Implementations in each service's `Data/Repositories` folder
- Enables easier testing and decoupling

**Dependency Injection**
- Microsoft.Extensions.DependencyInjection + Autofac
- Extension methods pattern for clean DI registration (see `ServiceExtensions/` folders)
- Autofac used for metadata-based parser registration

**Background Job Scheduling** (Hangfire)
- Each service has Hangfire dashboard at root URL (e.g., `/hangfire`)
- ParserService: Daily album parsing + outbox publishing jobs
- CatalogSyncService: Daily album processing + publishing jobs
- Jobs configured in `*SchedulerService` classes (IHostedService)

### Adding a New Parser

Parsers scrape distributor websites for album data. To add a new parser:

1. **Implement `IParser` interface** (see `MetalReleaseTracker.ParserService/Domain/Interfaces/IParser.cs`)
2. **Register parser** in `MetalReleaseTracker.ParserService/Infrastructure/Parsers/Extensions/ParserRegistrationExtension.cs`:
   ```csharp
   builder.RegisterType<YourNewParser>()
       .As<IParser>()
       .WithMetadata<ParserMetadata>(m =>
           m.For(meta => meta.DistributorCode, DistributorCode.YourDistributor));
   ```
3. **Add data source configuration** in `appsettings.json`:
   ```json
   "ParserDataSources": [
     {
       "DistributorCode": "YourDistributor",
       "Name": "yourdistributor.com",
       "ParsingUrl": "https://yourdistributor.com/releases"
     }
   ]
   ```
4. **Restart ParserService** - job is auto-registered by `ParserSchedulerService`

**Reference Implementation**: `MetalReleaseTracker.ParserService/Infrastructure/Parsers/OsmoseProductionsParser.cs`

### Data Storage Strategy

**PostgreSQL** (Relational)
- ParserService: Parsing sessions, outbox events (transactional)
- CoreDataService: Normalized album catalog (Albums, Bands, Distributors)
- Hangfire: Job scheduling state

**MongoDB** (NoSQL)
- CatalogSyncService: Temporary raw album storage with 30-day TTL
- Flexible schema for varying distributor data formats
- Processed albums stored until published to Kafka

**MinIO** (Object Storage)
- Album cover images
- Large JSON payloads (Kafka message size workaround)
- Pre-signed URLs generated by CoreDataService (1-day expiry)

### Testing Structure

**Integration Tests**
- Each service has tests in `Tests/` subfolder
- Uses Testcontainers for real database instances (PostgreSQL/MongoDB)
- Test organization: `Fixtures/` (setup), `Factories/` (test data), `IntegrationTests/` (tests)
- Focus on consumer/job/repository testing
- No unit tests currently (integration-first approach)

**Running Single Test**
```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~AlbumParsingJobTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~AlbumParsingJobTests.ParseAsync_Should_SaveAlbumParsedEvents"
```

### Configuration Files

**Important Settings** (appsettings.json)
- Kafka topics and bootstrap servers
- Database connection strings
- MinIO configuration (endpoint, credentials, bucket)
- Serilog + Seq logging configuration
- Hangfire cron schedules
- ParserDataSources (distributor URLs)

**User Secrets** (sensitive data)
- Google OAuth credentials (CoreDataService)
- Database passwords (production)
- MinIO credentials (production)

## Common Development Tasks

### Debugging Failed Jobs
1. Check Hangfire dashboard at service root URL (e.g., http://localhost:5000/hangfire)
2. View failed jobs tab for error details and stack traces
3. Check Seq logs at http://localhost:5341 for structured logging

### Monitoring Kafka Messages
1. Use Kafdrop UI at http://localhost:9003
2. View topics: `albums-parsed-topic`, `albums-processed-topic`
3. Inspect message payloads and consumer lag

### Viewing MinIO Files
1. Access MinIO Console at http://localhost:9002
2. Login: admin / S3cur3P@ssw0rd!
3. Browse `metal-release-tracker` bucket for images and JSON files

### Idempotency & Duplicate Prevention
- Albums identified by SKU (unique constraint in databases)
- Kafka consumers handle duplicate messages gracefully
- Outbox pattern prevents duplicate publishing

## Technology Stack

- **.NET 8.0** - All services target net8.0
- **Entity Framework Core 9.0** - ORM for PostgreSQL
- **MongoDB.Driver** - MongoDB client
- **MassTransit 8.3** - Kafka abstraction and messaging patterns
- **Hangfire** - Background job scheduling
- **HtmlAgilityPack** - HTML parsing for web scraping
- **FluentValidation** - Data validation
- **AutoMapper** - Object mapping
- **Serilog + Seq** - Structured logging
- **Testcontainers + xUnit** - Integration testing
- **StyleCop.Analyzers** - Code style enforcement
