# Docker Compose Guide

This guide explains how to run the Chess Tournaments application using Docker Compose.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine + Docker Compose (Linux)
- At least 4GB RAM available for Docker
- Ports 1433, 5000, 5001, and 5341 available on your host machine

## Quick Start

### Development Environment

1. **Clone the repository and navigate to the project root:**
   ```bash
   cd ChessTournaments
   ```

2. **Start all services:**
   ```bash
   docker compose up -d
   ```

3. **Wait for services to be healthy:**
   ```bash
   docker compose ps
   ```

4. **Access the services:**
   - API: http://localhost:5000
   - Identity Server: http://localhost:5001
   - Seq (Logging): http://localhost:5341
   - SQL Server: localhost:1433 (sa/YourStrong!Passw0rd)

5. **View logs:**
   ```bash
   docker compose logs -f
   ```

6. **Stop all services:**
   ```bash
   docker compose down
   ```

## Services

### SQL Server (sqlserver)
- **Image:** mcr.microsoft.com/mssql/server:2022-latest
- **Port:** 1433
- **Credentials:** sa / YourStrong!Passw0rd
- **Database:** ChessTournaments
- **Health Check:** Automatic health monitoring

### Identity Server (identity)
- **Port:** 5001
- **Purpose:** Authentication and authorization (OpenID Connect)
- **Dependencies:** SQL Server

### API (api)
- **Port:** 5000
- **Purpose:** Chess tournaments REST API
- **Dependencies:** SQL Server, Identity Server
- **Swagger:** http://localhost:5000/swagger

### Seq (seq)
- **Port:** 5341
- **Purpose:** Centralized logging and monitoring
- **UI:** http://localhost:5341

## Configuration

### Environment Variables

Create a `.env` file in the project root (copy from `.env.example`):

```bash
cp .env.example .env
```

Edit `.env` to customize:
- Database password
- Port mappings
- OIDC configuration

### Custom Configuration

The `docker-compose.override.yml` file is automatically merged and used for local development overrides. Edit this file to:
- Mount local configuration files
- Add development-specific environment variables
- Configure Seq logging

## Production Deployment

### Using Production Configuration

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Important Production Considerations

1. **Change default passwords:**
   - SQL Server SA password
   - OIDC client secrets
   - Seq admin password

2. **Use environment variables:**
   ```bash
   export SA_PASSWORD="YourProductionPassword"
   export OIDC_CLIENT_SECRET="YourProductionSecret"
   ```

3. **Enable HTTPS:**
   - Configure reverse proxy (nginx, traefik)
   - Use valid SSL certificates

4. **Configure persistent volumes:**
   - Backup SQL Server data volume
   - Backup Seq data volume

5. **Set resource limits:**
   - Already configured in `docker-compose.prod.yml`
   - Adjust based on your infrastructure

## Common Commands

### Build and Start
```bash
# Build images and start containers
docker compose up --build -d

# Start specific service
docker compose up api -d

# Scale a service
docker compose up --scale api=3 -d
```

### Monitoring
```bash
# View all logs
docker compose logs -f

# View specific service logs
docker compose logs -f api

# Check service status
docker compose ps

# View resource usage
docker stats
```

### Database Operations
```bash
# Connect to SQL Server
docker exec -it chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -C

# Backup database
docker exec chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "BACKUP DATABASE ChessTournaments TO DISK = N'/var/opt/mssql/backup/ChessTournaments.bak'" -C

# Restore database
docker cp backup.bak chess-tournaments-sqlserver:/var/opt/mssql/backup/
docker exec chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "RESTORE DATABASE ChessTournaments FROM DISK = N'/var/opt/mssql/backup/backup.bak' WITH REPLACE" -C
```

### Cleanup
```bash
# Stop and remove containers
docker compose down

# Stop and remove containers, volumes
docker compose down -v

# Remove all images
docker compose down --rmi all

# Full cleanup (containers, volumes, images)
docker compose down -v --rmi all
```

## Troubleshooting

### Service Won't Start

1. **Check logs:**
   ```bash
   docker compose logs <service-name>
   ```

2. **Check if ports are in use:**
   ```bash
   # Windows
   netstat -ano | findstr :5000

   # Linux/Mac
   lsof -i :5000
   ```

3. **Rebuild images:**
   ```bash
   docker compose build --no-cache
   docker compose up -d
   ```

### Database Connection Issues

1. **Verify SQL Server is healthy:**
   ```bash
   docker compose ps sqlserver
   ```

2. **Test connection:**
   ```bash
   docker exec -it chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT @@VERSION" -C
   ```

3. **Check connection string in services**

### Performance Issues

1. **Check resource usage:**
   ```bash
   docker stats
   ```

2. **Increase Docker Desktop resources:**
   - Settings → Resources → Advanced
   - Allocate more CPU/Memory

3. **Check container logs for errors:**
   ```bash
   docker compose logs -f
   ```

### API/Identity Server 502/503 Errors

1. **Wait for SQL Server to be fully ready** (can take 30-60 seconds on first start)

2. **Restart dependent services:**
   ```bash
   docker compose restart api identity
   ```

3. **Check if services are waiting for database migrations**

## Database Migrations

Migrations are automatically applied when the API starts. To manually run migrations:

```bash
# From the backend directory
dotnet ef database update --project src/ChessTournaments.Modules.Tournaments.Infrastructure --startup-project src/ChessTournaments.API
```

Or from within the container:
```bash
docker compose exec api dotnet ef database update
```

## Health Checks

All services have health checks configured. View health status:

```bash
docker compose ps
```

Healthy services show `healthy` in the STATUS column.

## Networking

All services communicate via the `chess-tournaments-network` bridge network. Services can reference each other by service name:

- API connects to Identity: `http://identity:8080`
- Services connect to SQL: `sqlserver:1433`
- Services connect to Seq: `http://seq:80`

## Volumes

### Persistent Data

- `sqlserver-data`: SQL Server database files
- `seq-data`: Seq logs and configuration

### Backup Volumes

```bash
# Backup SQL Server volume
docker run --rm -v chess-tournaments_sqlserver-data:/source -v $(pwd)/backups:/backup alpine tar czf /backup/sqlserver-backup.tar.gz -C /source .

# Restore SQL Server volume
docker run --rm -v chess-tournaments_sqlserver-data:/target -v $(pwd)/backups:/backup alpine tar xzf /backup/sqlserver-backup.tar.gz -C /target
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Test

on: [push]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Start services
        run: docker compose up -d

      - name: Wait for services
        run: docker compose ps

      - name: Run tests
        run: docker compose exec -T api dotnet test

      - name: Stop services
        run: docker compose down
```

## Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Docker](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)

## Support

For issues and questions:
- Check the logs: `docker compose logs -f`
- Review this documentation
- Open an issue on the repository
