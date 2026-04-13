# Chess Tournaments

A comprehensive chess tournament management system built with ASP.NET Core, featuring a modular monolith architecture with CQRS pattern.

## Features

- **Tournament Management**: Create and manage Swiss, Round Robin, and other tournament formats
- **Player Registration**: Register players with ratings and track their performance
- **Match Management**: Schedule and record match results
- **Authentication & Authorization**: OpenID Connect (OIDC) based identity server
- **RESTful API**: Clean, documented API endpoints
- **Centralized Logging**: Seq integration for structured logging
- **Docker Support**: Full containerization with Docker Compose
- **Reliable Messaging**: Outbox/Inbox patterns for guaranteed message delivery between modules

## Documentation

### Architecture & Design
- **[Architecture Guide](ARCHITECTURE.md)** - Detailed architecture overview, module structure, and design patterns
- **[Cross-Cutting Concerns](CROSS_CUTTING_CONCERNS.md)** - Shared infrastructure, exception handling, domain events, pagination
- **[Design & New Features](DESIGN_NEW_FEATURES.md)** - Feature design documentation and implementation guides

### Backend Development
- **[Entity Framework Core Guide](backend/ENTITY_FRAMEWORK_GUIDE.md)** - EF Core entity configuration patterns and best practices
- **[Database Migrations Guide](backend/MIGRATIONS_GUIDE.md)** - Creating and applying EF Core migrations
- **[Outbox/Inbox Pattern](backend/OUTBOX_INBOX_PATTERN.md)** - Reliable messaging implementation guide

### Infrastructure & Deployment
- **[Infrastructure README](infrastructure/README.md)** - Complete Azure infrastructure deployment guide
- **[GitHub Secrets Setup](infrastructure/GITHUB_SECRETS_SETUP.md)** - CI/CD secrets configuration for GitHub Actions
- **[Post-Deployment Setup](infrastructure/POST_DEPLOYMENT_SETUP.md)** - Manual role assignments (only if using Contributor role instead of Owner)

### Local Development
- **[Docker Guide](DOCKER.md)** - Comprehensive Docker setup, commands, and troubleshooting
- **[Secrets Migration](SECRETS_MIGRATION.md)** - Guide for migrating secrets and sensitive configuration

## Project Structure

```
ChessTournaments/
├── frontend/                                # Angular 19 frontend application
│   └── chess-tournaments-app/
├── backend/                                 # .NET backend
│   ├── src/
│   │   ├── ChessTournaments.API/           # Main API entry point
│   │   ├── ChessTournaments.Identity/      # OpenIddict Identity Server
│   │   ├── Shared/                         # Shared components
│   │   │   ├── ChessTournaments.Shared.Domain/           # Shared domain models (Outbox/Inbox)
│   │   │   └── ChessTournaments.Shared.IntegrationEvents/ # Event infrastructure
│   │   └── Modules/                        # Modular monolith modules
│   │       ├── Tournaments/                # Tournament management module
│   │       │   ├── Domain/                 # Domain models & business logic
│   │       │   ├── Application/            # CQRS commands/queries
│   │       │   ├── Infrastructure/         # EF Core, repositories
│   │       │   ├── IntegrationEvents/      # Module integration events
│   │       │   └── API/                    # Carter endpoints
│   │       ├── Matches/                    # Match management module
│   │       ├── Players/                    # Player management module
│   │       └── TournamentRequests/         # Tournament request module
│   ├── tests/
│   │   └── Modules/
│   │       ├── Tournaments/
│   │       │   ├── Unit/                   # TUnit unit tests
│   │       │   └── Integration/            # Integration & BDD tests
│   │       └── Matches/
│   │           └── Unit/                   # TUnit unit tests
│   ├── ENTITY_FRAMEWORK_GUIDE.md           # EF Core patterns & best practices
│   ├── MIGRATIONS_GUIDE.md                 # Database migrations guide
│   ├── OUTBOX_INBOX_PATTERN.md             # Messaging patterns guide
│   └── Directory.Packages.props            # Central package management
├── infrastructure/                          # Azure Infrastructure as Code
│   ├── bicep/                              # Bicep templates
│   │   ├── main.bicep                      # Main infrastructure template
│   │   ├── parameters.qa.json              # QA environment parameters
│   │   └── parameters.production.json      # Production parameters
│   ├── README.md                           # Infrastructure documentation
│   ├── POST_DEPLOYMENT_SETUP.md            # Post-deployment configuration
│   └── GITHUB_SECRETS_SETUP.md             # CI/CD secrets setup
├── .github/workflows/                       # GitHub Actions CI/CD
│   ├── qa.yml                              # QA deployment workflow
│   ├── production.yml                      # Production deployment workflow
│   ├── infrastructure-qa.yml               # Infrastructure deployment (QA)
│   └── infrastructure-production.yml       # Infrastructure deployment (Prod)
├── docker-compose.yml                       # Docker Compose configuration
├── ARCHITECTURE.md                          # Architecture documentation
├── DOCKER.md                                # Docker guide
└── README.md                                # This file
```

## Quick Start with Docker

The easiest way to run the entire application stack is using Docker Compose:

```bash
# Start all services (API, Identity, SQL Server, Seq)
docker compose up -d

# Check service status
docker compose ps

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

**Services will be available at:**
- **Frontend (Angular)**: http://localhost:4200
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Identity Server: http://localhost:5001
- Seq (Logging): http://localhost:5341
- SQL Server: localhost:1433

For detailed Docker instructions, see [DOCKER.md](DOCKER.md).

### Using Make Commands (Optional)

If you have `make` installed:

```bash
make up        # Start all services
make logs      # View logs
make ps        # Check status
make down      # Stop all services
make help      # Show all available commands
```

## Manual Setup (Without Docker)

### Prerequisites

- .NET 10.0 SDK
- .NET 8.0 SDK (for Identity server)
- SQL Server 2022 or later
- Node.js (for frontend)

### Backend Setup

1. **Configure the database:**

   Update connection strings in:
   - `backend/src/ChessTournaments.API/appsettings.json`
   - `backend/src/ChessTournaments.Identity/appsettings.json`

2. **Run database migrations:**

   ```bash
   cd backend
   dotnet ef database update --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure --startup-project src/ChessTournaments.API
   ```

3. **Start the Identity Server:**

   ```bash
   cd backend/src/ChessTournaments.Identity
   dotnet run
   ```

4. **Start the API:**

   ```bash
   cd backend/src/ChessTournaments.API
   dotnet run
   ```

### Frontend Setup

Navigate to the [frontend](frontend/) folder for frontend-specific instructions.

## Development

### Running Tests

```bash
# Run all tests
cd backend
dotnet test

# Run specific test project
dotnet test tests/Modules/Tournaments/Unit/ChessTournaments.Modules.Tournaments.UnitTests

# Run integration tests
dotnet test tests/Modules/Tournaments/Integration/ChessTournaments.Modules.Tournaments.IntegrationTests
```

### Code Formatting

The project uses CSharpier for consistent code formatting:

```bash
cd backend
dotnet csharpier format .
```

### Project Architecture

The backend follows a **modular monolith** architecture with:

- **Domain Layer**: Entities, value objects, domain events
- **Application Layer**: CQRS commands/queries, handlers, validators
- **Infrastructure Layer**: Data access, external services
- **API Layer**: HTTP endpoints using Carter minimal APIs

Key patterns:
- **CQRS** with MediatR
- **Domain-Driven Design** (DDD)
- **Repository Pattern**
- **FluentValidation** for input validation
- **Outbox Pattern** for reliable integration event publishing
- **Inbox Pattern** for idempotent event processing

#### Modular Architecture

The application is organized into four main modules:
- **Tournaments**: Tournament lifecycle management (creation, rounds, completion)
- **Matches**: Match scheduling and result recording
- **Players**: Player profiles and statistics
- **TournamentRequests**: Tournament request handling

Each module has its own:
- Database schema (separate tables per module)
- Domain models and business logic
- Integration events for cross-module communication
- Outbox/Inbox tables for reliable messaging

For detailed architecture information, see [ARCHITECTURE.md](ARCHITECTURE.md).

#### Reliable Messaging

The system uses the **Outbox/Inbox patterns** to ensure reliable message delivery:

- **Outbox Pattern**: Integration events are persisted to an outbox table in the same transaction as business data, then published by a background service
- **Inbox Pattern**: Received events are checked against an inbox table to ensure exactly-once processing (idempotency)
- **Background Processing**: OutboxMessageProcessor runs every 10 seconds, processing up to 20 messages per batch
- **Error Handling**: Failed messages are tracked with error details for debugging and retry

For implementation details, see [Outbox/Inbox Pattern Guide](backend/OUTBOX_INBOX_PATTERN.md).

## API Documentation

Once the API is running, access the interactive Swagger documentation at:
- http://localhost:5000/swagger

## Testing

The project includes comprehensive test coverage:

- **Unit Tests**: 70+ tests covering validators, handlers, and domain logic
- **Integration Tests**: API integration tests using WebApplicationFactory
- **SpecFlow Tests**: BDD-style scenarios for tournament lifecycle

Test frameworks used:
- TUnit (Modern .NET testing framework)
- FluentAssertions
- Moq
- SpecFlow
- Microsoft.AspNetCore.Mvc.Testing

## Cloud Deployment (Azure)

The application is designed for deployment to Microsoft Azure with full CI/CD automation.

### Azure Resources

The infrastructure includes:
- **App Service Plan** (Free/Premium tier) - Shared hosting plan for .NET applications
- **App Service (API)** - .NET 10 Backend API hosting
- **App Service (Identity)** - .NET 10 Blazor Server identity application
- **Static Web App (Frontend)** - Angular frontend application
- **Azure SQL Database** (Free tier: 32MB / Basic tier: 2GB) - Database
- **Key Vault** - Secrets management (connection strings, API keys)
- **Storage Account** - Blob storage
- **Service Bus** (Standard tier) - Message queue for integration events
- **Application Insights** - Monitoring & diagnostics
- **Log Analytics** - Centralized logging

**Note**: The API and Identity apps share the same App Service Plan for cost optimization.

### Deployment Guide

See [Infrastructure README](infrastructure/README.md) for complete deployment instructions:

1. **Prerequisites**: Azure subscription, GitHub repository
2. **Configure GitHub Secrets**: Azure credentials, SQL admin credentials
3. **Deploy Infrastructure**: Run infrastructure workflow to provision Azure resources
4. **Configure Role Assignments**: Assign Key Vault and Storage permissions
5. **Deploy Applications**: Automated deployment via GitHub Actions

### Environments

- **QA**: Sweden Central region (free tier resources)
- **Production**: West Europe region (premium tier resources)

### CI/CD Pipelines

GitHub Actions workflows in [.github/workflows/](.github/workflows/):

**Infrastructure Deployment:**
- `infrastructure-qa.yml` - Deploy QA infrastructure
- `infrastructure-production.yml` - Deploy Production infrastructure

**Application Deployment:**
- `qa.yml` - Build, test, and deploy to QA
- `production.yml` - Build, test, and deploy to Production
- `pull-requests.yml` - PR validation (build & test)

**Features:**
- Automated Bicep template deployment
- EF Core database migrations
- Backend API deployment to App Service
- Frontend deployment to Static Web Apps
- Automated testing before deployment
- Environment-specific configurations

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 10.0 / ASP.NET Core 8.0 (Identity Server)
- **Language**: C# 13
- **ORM**: Entity Framework Core 10.0
- **Database**: SQL Server 2022
- **Architecture Patterns**:
  - Modular Monolith (4 modules: Tournaments, Matches, Players, TournamentRequests)
  - CQRS with MediatR
  - Domain-Driven Design (DDD)
  - Repository Pattern
  - Outbox Pattern (reliable event publishing)
  - Inbox Pattern (idempotent event processing)
- **Messaging**:
  - Integration Events for cross-module communication
  - Background service for asynchronous event processing
  - Transactional outbox for guaranteed delivery
- **API Framework**: Carter (Minimal APIs)
- **Validation**: FluentValidation
- **Logging**: Serilog with Seq integration
- **Authentication**: OpenIddict (OpenID Connect/OAuth 2.0)
- **Result Pattern**: CSharpFunctionalExtensions
- **Code Formatting**: CSharpier

### Frontend
- **Framework**: Angular 19
- **Language**: TypeScript 5.7
- **UI Components**: PrimeNG (migrating from Angular Material)
- **State Management**: RxJS
- **HTTP Client**: Angular HttpClient
- **Authentication**: OAuth2/OIDC (angular-oauth2-oidc)
- **Build Tool**: Angular CLI with esbuild
- **Styling**: SCSS

### Testing
- **Unit/Integration Testing**: TUnit (Modern .NET testing framework)
- **BDD Testing**: SpecFlow
- **Assertions**: FluentAssertions
- **Mocking**: Moq
- **Web Testing**: Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)
- **Test Coverage**: 70+ unit tests, integration tests, and BDD scenarios

### Infrastructure & DevOps
- **Cloud Platform**: Microsoft Azure
  - **App Service**: Hosting for .NET API (Free/Premium tiers)
  - **Static Web Apps**: Hosting for Angular frontend
  - **Azure SQL Database**: SQL Server database (Free/Basic/Standard tiers)
  - **Key Vault**: Secure secrets management
  - **Storage Account**: Blob storage for file uploads
  - **Service Bus**: Message queue for async operations
  - **Application Insights**: Performance monitoring & diagnostics
  - **Log Analytics**: Centralized logging
- **Infrastructure as Code**: Azure Bicep
- **Containerization**: Docker & Docker Compose (local development)
- **Database**: SQL Server 2022 (Docker container for local, Azure SQL for cloud)
- **Logging & Monitoring**:
  - Seq (local development)
  - Application Insights (Azure)
- **CI/CD**: GitHub Actions
  - Automated infrastructure deployment
  - Backend deployment with EF Core migrations
  - Frontend deployment to Static Web Apps
  - Pull request validation
- **Health Checks**: ASP.NET Core Health Checks
- **API Documentation**: Swagger/OpenAPI

## Configuration

### Environment Variables

Key configuration values can be set via environment variables:

- `ConnectionStrings__DefaultConnection`: Database connection string
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Staging/Production)
- `Oidc__Authority`: Identity server URL
- `Oidc__API__ClientId`: API client ID
- `Oidc__API__ClientSecret`: API client secret

### appsettings.json

Configuration files:
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.Production.json`: Production settings

## Docker Support

Full Docker support with multi-stage builds:

- **Development**: `docker-compose.yml` + `docker-compose.override.yml`
- **Production**: `docker-compose.yml` + `docker-compose.prod.yml`

Features:
- Health checks for all services
- Automatic database initialization
- Centralized logging with Seq
- Volume persistence for data
- Network isolation

See [DOCKER.md](DOCKER.md) for comprehensive Docker documentation.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and code formatting
5. Submit a pull request

For implementing new features, refer to [DESIGN_NEW_FEATURES.md](DESIGN_NEW_FEATURES.md) for design patterns and best practices.

## License

(Add your license information here)

## Support

For issues and questions:
- Open an issue on GitHub
- Review the documentation:
  - [Architecture Guide](ARCHITECTURE.md)
  - [Cross-Cutting Concerns](CROSS_CUTTING_CONCERNS.md)
  - [Docker Guide](DOCKER.md)
  - [Design & New Features](DESIGN_NEW_FEATURES.md)
  - [Outbox/Inbox Pattern](backend/OUTBOX_INBOX_PATTERN.md)
  - [Secrets Migration](SECRETS_MIGRATION.md)
- Check the API documentation at /swagger
