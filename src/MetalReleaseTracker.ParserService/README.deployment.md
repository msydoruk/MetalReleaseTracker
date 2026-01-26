# Parser Service - Deployment Guide

## Quick Start

### 1. Setup Secrets

```bash
# Copy example to .env
cp .env.example .env

# Edit and set your passwords
notepad .env  # Windows
nano .env     # Linux/Mac
```

### 2. Deploy

```bash
# Development
cp .env.development .env
docker-compose up -d

# Production (set strong passwords first!)
cp .env.example .env
notepad .env  # Set STRONG passwords
docker-compose up -d
```

### 3. Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| Parser Service | http://localhost:5000 | - |
| TickerQ Dashboard | http://localhost:5000/tickerq/dashboard | admin / (from .env) |
| Kafdrop | http://localhost:9003 | - |
| Seq | http://localhost:5341 | - |
| MinIO Console | http://localhost:9002 | admin / (from .env) |

## Environment Files

- `.env.example` - Template (commit to git)
- `.env.development` - Dev secrets (commit to git, weak passwords OK)
- `.env` - Active file (DO NOT commit, add to .gitignore)

## Commands

```bash
# View logs
docker-compose logs -f parser-service

# Restart service
docker-compose restart parser-service

# Stop all
docker-compose stop

# Remove everything (including data!)
docker-compose down -v

# Rebuild after code changes
docker-compose up -d --build parser-service
```

## Secrets Required

- `POSTGRES_PASSWORD` - PostgreSQL admin password
- `MINIO_ROOT_PASSWORD` - MinIO admin password
- `TICKERQ_DASHBOARD_PASSWORD` - TickerQ dashboard password

## Production Checklist

- [ ] Change ALL passwords in `.env`
- [ ] Use strong passwords (16+ chars)
- [ ] Verify `.env` is in `.gitignore`
- [ ] Configure firewall rules
- [ ] Setup SSL/HTTPS (via reverse proxy)
- [ ] Configure backups

## Troubleshooting

```bash
# Check logs
docker-compose logs parser-service

# Verify environment
docker-compose config

# Test connection
docker-compose exec postgres psql -U admin -d ParserServiceDb -c "SELECT 1"
```
