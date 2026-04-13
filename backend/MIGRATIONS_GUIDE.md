# Database Migrations Guide

This document provides guidance on creating and applying EF Core migrations for the Chess Tournaments modular monolith.

## Overview

The application uses **EF Core Migrations** to manage database schema changes. Each module has its own DbContext and migration history, stored in separate database schemas.

## Module Schemas

- **Tournaments**: `Tournaments` schema
- **Matches**: `Matches` schema (when implemented)
- **Players**: `Players` schema (when implemented)
- **TournamentRequests**: `TournamentRequests` schema (when implemented)

## Creating Migrations

### Tournaments Module

```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext
```

### Matches Module

```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Matches/ChessTournaments.Modules.Matches.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context MatchesDbContext
```

### Players Module

```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Players/ChessTournaments.Modules.Players.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context PlayersDbContext
```

### TournamentRequests Module

```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project src/Modules/TournamentRequests/ChessTournaments.Modules.TournamentRequests.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentRequestsDbContext
```

## Applying Migrations

### Development (Automatic)

In development, migrations are automatically applied when the application starts. See `WebApplicationExtensions.ApplyModuleMigrations()` in the API project.

### Production (Manual)

For production, apply migrations manually before deploying:

```bash
# Tournaments module
dotnet ef database update \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext

# Repeat for other modules...
```

### Docker Environments

Migrations are automatically applied when containers start in development mode. For production Docker deployments, consider:

1. Running migrations in an init container
2. Using a migration job before deployment
3. Including migration in the deployment pipeline

## Recent Migrations

### Tournaments Module

#### AddOutboxInboxMessages (2026-01-01)

Added Outbox/Inbox pattern support for reliable messaging:

**Tables Created:**
- `Tournaments.OutboxMessages` - Stores outgoing integration events
- `Tournaments.InboxMessages` - Tracks incoming integration events for idempotency

**Schema:**
```sql
CREATE TABLE [Tournaments].[OutboxMessages] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [Type] nvarchar(500) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [OccurredOnUtc] datetime2 NOT NULL,
    [ProcessedOnUtc] datetime2 NULL,
    [Error] nvarchar(2000) NULL
);

CREATE INDEX [IX_OutboxMessages_OccurredOnUtc] ON [Tournaments].[OutboxMessages] ([OccurredOnUtc]);
CREATE INDEX [IX_OutboxMessages_ProcessedOnUtc] ON [Tournaments].[OutboxMessages] ([ProcessedOnUtc]);

CREATE TABLE [Tournaments].[InboxMessages] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
    [Type] nvarchar(500) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [OccurredOnUtc] datetime2 NOT NULL,
    [ProcessedOnUtc] datetime2 NULL,
    [Error] nvarchar(2000) NULL
);

CREATE INDEX [IX_InboxMessages_OccurredOnUtc] ON [Tournaments].[InboxMessages] ([OccurredOnUtc]);
CREATE INDEX [IX_InboxMessages_ProcessedOnUtc] ON [Tournaments].[InboxMessages] ([ProcessedOnUtc]);
```

**Purpose:**
- Ensures reliable delivery of integration events between modules
- Provides idempotency protection against duplicate event processing
- Enables message retry and error tracking

## Migration Best Practices

### 1. Naming Conventions

Use descriptive names that indicate what the migration does:
- ✅ `AddOutboxInboxMessages`
- ✅ `UpdateTournamentSchema`
- ✅ `AddPlayerRatingIndex`
- ❌ `Migration1`
- ❌ `Update`

### 2. Review Before Applying

Always review the generated migration code before applying:

```bash
# View the migration file
cat src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure/Migrations/<timestamp>_<MigrationName>.cs
```

### 3. Data Migrations

For data migrations, add custom SQL in the migration's `Up` method:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Schema changes
    migrationBuilder.CreateTable(...);

    // Data migration
    migrationBuilder.Sql(@"
        UPDATE Tournaments.Tournaments
        SET Status = 'Active'
        WHERE Status IS NULL
    ");
}
```

### 4. Rollback Strategy

Always test the `Down` method to ensure migrations can be rolled back:

```bash
# Rollback last migration
dotnet ef database update <PreviousMigrationName> \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext

# Or remove the last migration (if not applied)
dotnet ef migrations remove \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext
```

### 5. Schema Separation

Each module maintains its own schema and migration history:
- Never reference tables from other modules in migrations
- Use integration events for cross-module data needs
- Keep foreign keys within the same schema

### 6. Index Strategy

Add indexes for:
- Foreign keys (usually created automatically)
- Frequently queried columns
- Columns used in WHERE, ORDER BY, or JOIN clauses
- Outbox/Inbox `ProcessedOnUtc` and `OccurredOnUtc` columns

### 7. Production Considerations

Before applying migrations to production:

1. **Backup the database**
2. **Test migrations on a staging environment**
3. **Review migration scripts** for potential issues
4. **Plan for downtime** if breaking changes are involved
5. **Have a rollback plan** ready

## Troubleshooting

### Migration Already Applied

If you see "Migration 'XXX' has already been applied to the database":
- The migration is already in the database
- Check `[Schema].__EFMigrationsHistory` table
- Use `dotnet ef database update` without a migration name to apply pending migrations

### Multiple DbContexts

If you get "More than one DbContext was found":
- Always specify `--context <DbContextName>` parameter
- Each module has its own DbContext

### Connection String Issues

If migrations fail to connect:
- Verify connection string in `appsettings.json`
- Ensure SQL Server is running
- Check firewall/network settings
- For Docker, ensure containers are running: `docker compose up -d`

### EF Core Tools Version

If you see version warnings:
```
The Entity Framework tools version 'X.X.X' is older than that of the runtime 'Y.Y.Y'
```

Update EF Core tools:
```bash
dotnet tool update --global dotnet-ef
```

## Adding Outbox/Inbox to Other Modules

To add Outbox/Inbox pattern support to other modules:

1. **Update DbContext** to include OutboxMessage and InboxMessage DbSets:

```csharp
public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
```

2. **Create EF Core configurations** in `Persistence/Configurations/`:
   - `OutboxMessageConfiguration.cs`
   - `InboxMessageConfiguration.cs`

3. **Create migration**:

```bash
dotnet ef migrations add AddOutboxInboxMessages \
  --project src/Modules/<ModuleName>/ChessTournaments.Modules.<ModuleName>.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context <ModuleName>DbContext
```

4. **Register services** in the module's registration file:

```csharp
services.AddOutboxInboxPatterns<ModuleDbContext>();
services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
```

5. **Apply migration**:

```bash
dotnet ef database update \
  --project src/Modules/<ModuleName>/ChessTournaments.Modules.<ModuleName>.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context <ModuleName>DbContext
```

## References

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Managing Migrations in Production](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Outbox/Inbox Pattern Guide](OUTBOX_INBOX_PATTERN.md)
