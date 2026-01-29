# Deployment Guide

## Prerequisites

**SSH Key Authentication:**
- You need an SSH private key file (`.pem`) to connect to your server
- For AWS EC2: Download key when creating instance
- Replace `path\to\your-key.pem` with actual path (e.g., `g:\My Drive\keys\MyKey.pem`)
- Replace `YOUR_SERVER_IP` with your server's public IP address

**Optional: Simplify SSH connections with config file**

Create `C:\Users\YourUsername\.ssh\config`:
```
Host metaltracker
    HostName YOUR_SERVER_IP
    User ubuntu
    IdentityFile path\to\your-key.pem
```

Then use: `ssh metaltracker` instead of full command.

---

## Local Development

```bash
# Build and start all services (Kafka, PostgreSQL, MinIO, Seq, ParserService)
docker compose up -d

# View logs from parser service to check if it's running
docker compose logs -f parser-service

# Stop all services
docker compose down
```

**Access dashboards locally:**
- TickerQ: http://localhost:5000/tickerq/dashboard (user: admin, pass: AdminPass123!)
- Kafdrop: http://localhost:9003
- Seq: http://localhost:5341
- MinIO: http://localhost:9002 (user: minio_admin, pass: MinioPass123!)

---

## Production Deployment (Ubuntu Server)

### Step 1: Prepare Server

```bash
# Connect to your server via SSH
ssh ubuntu@YOUR_SERVER_IP

# Update package list to get latest versions
sudo apt update

# Install prerequisites needed to add Docker's repository
sudo apt install -y ca-certificates curl gnupg lsb-release

# Add Docker's official GPG key (verifies package authenticity)
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Add Docker repository to apt sources
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Update package list with Docker's repository
sudo apt update

# Install Docker and Docker Compose plugin from official Docker repository
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Add your user to docker group so you can run docker without sudo
sudo usermod -aG docker $USER

# Exit and reconnect for group changes to take effect
exit
```

**Reconnect and verify:**
```bash
# SSH back into server (use your key file)
ssh -i "path\to\your-key.pem" ubuntu@YOUR_SERVER_IP

# Verify Docker is installed correctly (should work without sudo now)
docker --version
docker compose version

# Test Docker works
docker ps
```

### Step 2: Upload Code to Server

```powershell
# From your local machine (PowerShell), navigate to project root
cd c:\Works\Personal\MetalReleaseTrackerMicroservices

# Delete unnecessary folders before upload (bin, obj, node_modules, .idea, Tests)
# This significantly reduces upload size and time
Get-ChildItem -Path src -Recurse -Directory -Include bin,obj,node_modules,.idea,Tests | Remove-Item -Recurse -Force

# Create directory on server first (replace YOUR_KEY_FILE and YOUR_SERVER_IP)
ssh -i "path\to\your-key.pem" ubuntu@YOUR_SERVER_IP "mkdir -p /home/ubuntu/metaltracker"

# Upload entire src directory to server using SCP
# This copies all source code, Dockerfile, docker-compose.yml, etc.
scp -i "path\to\your-key.pem" -r src ubuntu@YOUR_SERVER_IP:/home/ubuntu/metaltracker/
```

**Example with actual key path:**
```powershell
# If your key is at: g:\My Drive\keys\MyKey.pem
Get-ChildItem -Path src -Recurse -Directory -Include bin,obj,node_modules,.idea,Tests | Remove-Item -Recurse -Force
ssh -i "g:\My Drive\keys\MyKey.pem" ubuntu@YOUR_SERVER_IP "mkdir -p /home/ubuntu/metaltracker"
scp -i "g:\My Drive\keys\MyKey.pem" -r src ubuntu@YOUR_SERVER_IP:/home/ubuntu/metaltracker/
```

### Step 3: Configure Environment

```bash
# Connect to server (use your key file)
ssh -i "path\to\your-key.pem" ubuntu@YOUR_SERVER_IP

# Navigate to ParserService directory
cd /home/ubuntu/metaltracker/src/MetalReleaseTracker.ParserService

# Edit .env file with production values
nano .env
```

**Change these values in .env:**
```bash
# Set to your server's public IP (needed for Kafka external access)
PUBLIC_IP=YOUR_SERVER_IP

# Use strong passwords for production (replace DevPassword123!)
POSTGRES_PASSWORD=YourStrongPassword1
MINIO_ROOT_PASSWORD=YourStrongPassword2
TICKERQ_DASHBOARD_PASSWORD=YourStrongPassword3
```

**Save file:** Press `Ctrl+O`, then `Enter`, then `Ctrl+X`

```bash
# Set secure permissions so only you can read the .env file with passwords
chmod 600 .env
```

### Step 4: Build and Run

```bash
# Build Docker image for parser-service (compiles .NET code into container)
docker compose build

# Start all services in background (-d means detached mode)
# Docker will start: Zookeeper, Kafka, PostgreSQL, MinIO, Seq, and ParserService
docker compose up -d

# Watch logs in real-time to verify everything started correctly
docker compose logs -f parser-service
```

**Look for these success messages in logs:**
- "Application started"
- "Database migration completed"
- No error messages

Press `Ctrl+C` to stop watching logs (services keep running)

### Step 5: Configure Firewall

```bash
# Open port 5000 for TickerQ dashboard access
sudo ufw allow 5000/tcp

# Open port 9003 for Kafdrop (Kafka UI)
sudo ufw allow 9003/tcp

# Open port 5341 for Seq (logs dashboard)
sudo ufw allow 5341/tcp

# Open port 9002 for MinIO console
sudo ufw allow 9002/tcp

# Enable firewall if not already active
sudo ufw enable

# Check firewall status and confirm ports are open
sudo ufw status
```

### Step 6: Verify Deployment

```bash
# Check all containers are running and healthy
docker compose ps

# Check parser service logs for errors
docker compose logs parser-service | tail -50
```

**Access dashboards from browser** (replace YOUR_IP with your server IP):

1. **TickerQ**: http://YOUR_IP:5000/tickerq/dashboard
   - Login with TICKERQ_DASHBOARD_USERNAME and PASSWORD from .env
   - Check scheduled parsing jobs

2. **Kafdrop**: http://YOUR_IP:9003
   - View Kafka topics
   - Check for messages in `albums-parsed-topic`

3. **Seq**: http://YOUR_IP:5341
   - View application logs in real-time
   - Search and filter logs

4. **MinIO**: http://YOUR_IP:9002
   - Login with MINIO_ROOT_USER and PASSWORD from .env
   - Browse uploaded album images

```bash
# Connect to PostgreSQL and check if albums are being parsed
docker compose exec postgres psql -U parser_admin -d ParserServiceDb

# Inside PostgreSQL, run this query to count albums
SELECT COUNT(*) FROM "Albums";

# Exit PostgreSQL
\q
```

---

## Useful Commands

```bash
# View logs from all services
docker compose logs

# View logs from specific service only
docker compose logs parser-service

# Follow logs in real-time (-f means follow, like tail -f)
docker compose logs -f

# Restart just the parser service without affecting other containers
docker compose restart parser-service

# Restart all services
docker compose restart

# Stop all services but keep data (volumes persist)
docker compose down

# Stop all services and DELETE all data (removes volumes with database data)
docker compose down -v

# Check status of all containers (running/stopped/healthy)
docker compose ps

# Check resource usage (CPU, RAM, network)
docker stats
```

---

## Update Code After Changes

```powershell
# From local machine, navigate to project root
cd c:\Works\Personal\MetalReleaseTrackerMicroservices

# Delete unnecessary folders before upload
Get-ChildItem -Path src -Recurse -Directory -Include bin,obj,node_modules,.idea,Tests | Remove-Item -Recurse -Force

# Upload updated code (use your key file)
scp -i "path\to\your-key.pem" -r src ubuntu@YOUR_SERVER_IP:/home/ubuntu/metaltracker/
```

```bash
# On server
ssh -i "path\to\your-key.pem" ubuntu@YOUR_SERVER_IP
cd /home/ubuntu/metaltracker/src/MetalReleaseTracker.ParserService

# Rebuild Docker image without using cache (ensures fresh build)
docker compose build --no-cache

# Restart services with new image
docker compose up -d
```

---

## Backup Database

```bash
# Create backup of PostgreSQL database
# This exports all data to a SQL file with current date in filename
docker compose exec postgres pg_dump -U parser_admin ParserServiceDb > backup_$(date +%Y%m%d).sql

# Restore from backup (if needed)
docker compose exec -T postgres psql -U parser_admin -d ParserServiceDb < backup_20260129.sql
```

---

## Troubleshooting

### Parser service won't start

```bash
# Check detailed logs for error messages
docker compose logs parser-service

# Common issues:
# - Database connection failed: Check POSTGRES_PASSWORD in .env
# - MinIO connection failed: Check MINIO_ROOT_PASSWORD in .env
# - Port already in use: Check if port 5000 is used by another app
```

### Kafka connection issues

```bash
# Verify Kafka is running and healthy
docker compose ps kafka

# Check Kafka logs for errors
docker compose logs kafka

# Ensure PUBLIC_IP is set correctly in .env (use server's actual IP, not localhost)
```

### Database not initializing

```bash
# Check PostgreSQL logs
docker compose logs postgres

# Try connecting manually to test credentials
docker compose exec postgres psql -U parser_admin -d ParserServiceDb
```

### Out of disk space

```bash
# Check disk usage
df -h

# Clean up unused Docker images and containers (frees space)
docker system prune -a

# Remove old logs
docker compose logs --tail=0
```

---

## Files Structure

**Required for deployment:**
- `docker-compose.yml` - Defines all services (Kafka, DB, MinIO, ParserService)
- `.env` - Environment variables (passwords, IP)
- `Dockerfile` - Instructions to build ParserService container
- `Program.cs`, `appsettings.json` - Application code and config
- `MetalReleaseTracker.ParserService.csproj` - .NET project file
- Folders: `Aplication/`, `Domain/`, `Infrastructure/`, `Properties/` - Source code

**Not needed on server (dev only):**
- `bin/`, `obj/` - Build artifacts (generated during compilation)
- `.idea/` - IDE settings
- `Tests/` - Unit/integration tests
- `appsettings.Development.json` - Development configuration
