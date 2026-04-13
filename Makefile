.PHONY: help build up down restart logs ps clean test migrate backup restore

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

build: ## Build all Docker images
	docker compose build

up: ## Start all services
	docker compose up -d

down: ## Stop all services
	docker compose down

restart: ## Restart all services
	docker compose restart

logs: ## Show logs from all services
	docker compose logs -f

logs-api: ## Show logs from API service
	docker compose logs -f api

logs-identity: ## Show logs from Identity service
	docker compose logs -f identity

logs-db: ## Show logs from SQL Server
	docker compose logs -f sqlserver

ps: ## Show status of all services
	docker compose ps

clean: ## Remove all containers, volumes, and images
	docker compose down -v --rmi all

clean-volumes: ## Remove only volumes (keeps containers and images)
	docker compose down -v

test: ## Run tests inside the API container
	docker compose exec api dotnet test

migrate: ## Run database migrations
	docker compose exec api dotnet ef database update

backup-db: ## Backup the database
	@mkdir -p ./backups
	docker exec chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "BACKUP DATABASE ChessTournaments TO DISK = N'/var/opt/mssql/backup/ChessTournaments_$$(date +%Y%m%d_%H%M%S).bak'" -C

restore-db: ## Restore the database (requires BACKUP_FILE variable)
	@if [ -z "$(BACKUP_FILE)" ]; then echo "Error: BACKUP_FILE not set. Usage: make restore-db BACKUP_FILE=path/to/backup.bak"; exit 1; fi
	docker cp $(BACKUP_FILE) chess-tournaments-sqlserver:/var/opt/mssql/backup/restore.bak
	docker exec chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "RESTORE DATABASE ChessTournaments FROM DISK = N'/var/opt/mssql/backup/restore.bak' WITH REPLACE" -C

shell-api: ## Open shell in API container
	docker compose exec api /bin/bash

shell-db: ## Open SQL Server shell
	docker exec -it chess-tournaments-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -C

health: ## Check health status of all services
	@docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Health}}"

prod-up: ## Start services with production configuration
	docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

prod-down: ## Stop services running in production mode
	docker compose -f docker-compose.yml -f docker-compose.prod.yml down

rebuild: ## Rebuild and restart all services
	docker compose down
	docker compose build --no-cache
	docker compose up -d

stats: ## Show resource usage statistics
	docker stats --no-stream

prune: ## Clean up unused Docker resources
	docker system prune -af --volumes
