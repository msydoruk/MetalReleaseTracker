# MetalReleaseTracker.SharedInfrastructure

Centralized shared infrastructure for the Metal Release Tracker microservices platform.

## Overview

This project contains the **shared Docker infrastructure** used by all Metal Release Tracker services. It provides event-driven messaging, distributed logging, and object storage capabilities.

### Included Components

1. **Kafka** - Event streaming platform for inter-service communication
   - Zookeeper for Kafka coordination
   - Kafdrop UI for Kafka monitoring
2. **Seq** - Centralized distributed logging for tracing inter-service interactions via `CorrelationId`/`TraceId`
3. **MinIO** - Shared object storage for album images and large JSON payloads

### What's NOT Included

- **Databases** - Each service manages its own database(s):
  - ParserService: PostgreSQL (EF Core)
  - CatalogSyncService: MongoDB + Hangfire PostgreSQL
  - CoreDataService: PostgreSQL (EF Core)
- **Service Schedulers** - Each service manages its own scheduler (Hangfire/TickerQ)
- **MinIO Buckets** - Services create their own buckets on startup using provided credentials

## Quick Start

### 1. Configuration

Copy the environment template and fill in your values:

```bash
cd src/MetalReleaseTracker.SharedInfrastructure
cp .env.template .env
```

Edit `.env` and set:
- `PUBLIC_IP` - Your server IP (use `localhost` for local development)
- `SEQ_ADMIN_PASSWORD` - Secure password for Seq admin console
- `MINIO_ROOT_USER` - MinIO admin username (default: `admin`)
- `MINIO_ROOT_PASSWORD` - Secure password for MinIO

### 2. Start Infrastructure

```bash
docker-compose up -d
```

### 3. Verify Services

Check that all services are healthy:

```bash
docker-compose ps
```

You should see:
- ✅ metalrelease-zookeeper (healthy)
- ✅ metalrelease-kafka (healthy)
- ✅ metalrelease-kafdrop (running)
- ✅ metalrelease-seq (running)
- ✅ metalrelease-minio (healthy)

### 4. Access Web UIs

- **Kafdrop** (Kafka UI): http://localhost:9003
- **Seq** (Logging): http://localhost:5341
- **MinIO Console**: http://localhost:9002

## Service Integration

Services consume shared infrastructure via:

1. **Docker Network**: `metalrelease_shared_net`
2. **Environment Variables**: Service-specific configuration
3. **Stable DNS Names**: `kafka`, `seq`, `minio`

### Example Service Configuration

Each service's `docker-compose.yml` should:

1. **Join the shared network**:
   ```yaml
   networks:
     - metalrelease_shared_net

   networks:
     metalrelease_shared_net:
       external: true
   ```

2. **Configure environment variables**:
   ```yaml
   environment:
     # Kafka
     - Kafka__BootstrapServers=kafka:9092

     # Seq
     - Serilog__WriteTo__1__Args__serverUrl=http://seq:80

     # MinIO
     - MinIO__Endpoint=minio:9001
     - MinIO__AccessKey=${MINIO_ROOT_USER}
     - MinIO__SecretKey=${MINIO_ROOT_PASSWORD}
     - MinIO__BucketName=your-service-bucket
     - MinIO__Region=us-east-1
   ```

3. **Ensure bucket creation on startup**:
   Services must create their own MinIO buckets using the provided credentials.

## Architecture

### Network Topology

```
┌─────────────────────────────────────────────────────┐
│        metalrelease_shared_net (Bridge)             │
│                                                     │
│  ┌─────────┐  ┌────────┐  ┌─────┐  ┌────────┐    │
│  │ Kafka   │  │  Seq   │  │MinIO│  │Kafdrop │    │
│  │ :9092   │  │ :5341  │  │:9001│  │ :9003  │    │
│  └─────────┘  └────────┘  └─────┘  └────────┘    │
│                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────┐│
│  │ParserService │  │CatalogSync   │  │CoreData  ││
│  │  (joins)     │  │Service       │  │Service   ││
│  └──────────────┘  └──────────────┘  └──────────┘│
└─────────────────────────────────────────────────────┘
```

### Port Mapping

| Service   | Internal Port | External Port | Purpose                |
|-----------|---------------|---------------|------------------------|
| Zookeeper | 2181          | 2181          | Kafka coordination     |
| Kafka     | 9092          | 9092          | Internal broker        |
| Kafka     | 9093          | 9093          | External broker        |
| Kafdrop   | 9000          | 9003          | Kafka UI               |
| Seq       | 80            | 5341          | Logging dashboard      |
| MinIO     | 9001          | 9001          | S3-compatible API      |
| MinIO     | 9002          | 9002          | Web console            |

### Persistent Volumes

All data is stored in named Docker volumes for persistence:

- `metalrelease_zookeeper_data` - Zookeeper state
- `metalrelease_zookeeper_logs` - Zookeeper logs
- `metalrelease_kafka_data` - Kafka topics and partitions
- `metalrelease_seq_data` - Seq logs and configuration
- `metalrelease_minio_data` - Object storage (buckets created by services)

## Operations

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f kafka
docker-compose logs -f seq
docker-compose logs -f minio
```

### Stop Infrastructure

```bash
docker-compose down
```

### Stop and Remove Volumes (⚠️ Data Loss)

```bash
docker-compose down -v
```

### Restart Services

```bash
docker-compose restart
```

## Troubleshooting

### Kafka Not Starting

1. Ensure Zookeeper is healthy:
   ```bash
   docker-compose ps zookeeper
   ```

2. Check Zookeeper logs:
   ```bash
   docker-compose logs zookeeper
   ```

3. Verify port 2181 is not in use:
   ```bash
   netstat -an | findstr 2181
   ```

### MinIO Not Accessible

1. Check MinIO health:
   ```bash
   docker-compose exec minio curl -f http://localhost:9001/minio/health/live
   ```

2. Verify credentials in `.env`:
   - `MINIO_ROOT_USER`
   - `MINIO_ROOT_PASSWORD`

### Service Cannot Join Network

1. Verify network exists:
   ```bash
   docker network ls | findstr metalrelease_shared_net
   ```

2. Ensure SharedInfrastructure is running:
   ```bash
   cd src/MetalReleaseTracker.SharedInfrastructure
   docker-compose up -d
   ```

3. Service's `docker-compose.yml` must declare external network:
   ```yaml
   networks:
     metalrelease_shared_net:
       external: true
   ```

## Production Deployment

### Security Checklist

- [ ] Set strong passwords in `.env.prod`
- [ ] Use `PUBLIC_IP` with your actual server IP
- [ ] Ensure `.env` and `.env.prod` are in `.gitignore`
- [ ] Restrict MinIO access to internal network only
- [ ] Configure Seq authentication (SEQ_FIRSTRUN_ADMINPASSWORD)
- [ ] Review Kafka listener security settings

### Backup Strategy

Persistent volumes should be backed up regularly:

```bash
# Example: Backup MinIO data
docker run --rm -v metalrelease_minio_data:/data -v $(pwd):/backup ubuntu tar czf /backup/minio_backup.tar.gz /data

# Example: Backup Kafka data
docker run --rm -v metalrelease_kafka_data:/data -v $(pwd):/backup ubuntu tar czf /backup/kafka_backup.tar.gz /data
```

## Development vs Production

### Development (.env)

```env
PUBLIC_IP=localhost
SEQ_ADMIN_PASSWORD=devpassword
MINIO_ROOT_USER=admin
MINIO_ROOT_PASSWORD=S3cur3P@ssw0rd!
```

### Production (.env.prod)

```env
PUBLIC_IP=your.server.ip
SEQ_ADMIN_PASSWORD=<strong-random-password>
MINIO_ROOT_USER=admin
MINIO_ROOT_PASSWORD=<strong-random-password>
```

## References

- [Kafka Documentation](https://kafka.apache.org/documentation/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [MinIO Documentation](https://min.io/docs/minio/linux/index.html)
- [MassTransit Kafka Integration](https://masstransit.io/documentation/transports/kafka)
