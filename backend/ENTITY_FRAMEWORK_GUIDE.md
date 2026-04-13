# Entity Framework Core Configuration Guide

This guide provides best practices and patterns for configuring Entity Framework Core in the Chess Tournaments modular monolith.

## Table of Contents
1. [Overview](#overview)
2. [Entity Configuration Basics](#entity-configuration-basics)
3. [Configuration Patterns](#configuration-patterns)
4. [Value Object Configuration](#value-object-configuration)
5. [Relationship Configuration](#relationship-configuration)
6. [Advanced Configurations](#advanced-configurations)
7. [Best Practices](#best-practices)
8. [Common Patterns](#common-patterns)

## Overview

The application uses **Entity Framework Core 10.0** with the Fluent API for entity configuration. Each module has its own `DbContext` and entity configurations stored in separate schema namespaces.

### Why Fluent API Over Attributes?

The project uses Fluent API instead of data annotations because:
- ✅ Keeps domain entities clean and free from infrastructure concerns
- ✅ Provides more configuration options
- ✅ Allows complex configurations not possible with attributes
- ✅ Separates domain logic from persistence concerns
- ✅ Better suited for Domain-Driven Design (DDD)

## Entity Configuration Basics

### Creating a Configuration Class

All entity configurations implement `IEntityTypeConfiguration<TEntity>`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        // Configuration goes here
    }
}
```

### Registering Configurations in DbContext

Configurations are automatically discovered and applied:

```csharp
public class TournamentsDbContext : DbContext
{
    public DbSet<Tournament> Tournaments => Set<Tournament>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TournamentsDbContext).Assembly);

        // Or apply specific configurations
        // modelBuilder.ApplyConfiguration(new TournamentConfiguration());
    }
}
```

## Configuration Patterns

### 1. Table and Primary Key Configuration

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Specify table name (and optionally schema)
    builder.ToTable("Tournaments");
    // or with schema: builder.ToTable("Tournaments", "Tournaments");

    // Configure primary key
    builder.HasKey(t => t.Id);

    // Composite key (if needed)
    // builder.HasKey(t => new { t.TournamentId, t.PlayerId });
}
```

### 2. Property Configuration

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Required string with max length
    builder.Property(t => t.Name)
        .IsRequired()
        .HasMaxLength(200);

    // Optional string with max length
    builder.Property(t => t.Description)
        .HasMaxLength(2000);

    // Required property (value type)
    builder.Property(t => t.StartDate)
        .IsRequired();

    // Optional property (nullable value type)
    builder.Property(t => t.EndDate); // No .IsRequired() means nullable

    // Enum stored as string
    builder.Property(t => t.Status)
        .HasConversion<string>()
        .HasMaxLength(50);

    // Decimal with precision
    builder.Property(t => t.EntryFee)
        .HasColumnType("decimal(18,2)");

    // User ID from Auth0 (external identity)
    builder.Property(t => t.OrganizerId)
        .IsRequired()
        .HasMaxLength(450);
}
```

### 3. Index Configuration

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Simple index
    builder.HasIndex(t => t.OrganizerId);

    // Composite index
    builder.HasIndex(t => new { t.Status, t.StartDate });

    // Unique index
    builder.HasIndex(t => t.Name)
        .IsUnique();

    // Filtered index (SQL Server)
    builder.HasIndex(t => t.Status)
        .HasFilter("[Status] IS NOT NULL");

    // Named index
    builder.HasIndex(t => t.StartDate)
        .HasDatabaseName("IX_Tournaments_StartDate");
}
```

## Value Object Configuration

Value Objects are configured using `OwnsOne()` or `OwnsMany()`:

### Single Value Object (Owned Entity)

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Configure owned entity (value object)
    builder.OwnsOne(
        t => t.Settings,
        s =>
        {
            // Properties of the value object
            s.Property(x => x.Format)
                .HasConversion<string>()
                .HasMaxLength(50);

            s.Property(x => x.TimeControl)
                .HasConversion<string>()
                .HasMaxLength(50);

            s.Property(x => x.TimeInMinutes)
                .IsRequired();

            s.Property(x => x.IncrementInSeconds)
                .IsRequired();

            s.Property(x => x.NumberOfRounds)
                .IsRequired();

            s.Property(x => x.MaxPlayers)
                .IsRequired();

            s.Property(x => x.MinPlayers)
                .IsRequired();

            s.Property(x => x.AllowByes)
                .IsRequired();

            s.Property(x => x.EntryFee)
                .HasColumnType("decimal(18,2)");
        }
    );
}
```

**Database Mapping**: Owned entities are stored in the same table as the owner by default, with columns prefixed by the property name (e.g., `Settings_Format`, `Settings_TimeControl`).

### Customizing Owned Entity Column Names

```csharp
builder.OwnsOne(
    t => t.Settings,
    s =>
    {
        // Custom column name
        s.Property(x => x.Format)
            .HasColumnName("TournamentFormat");

        // Or remove prefix
        s.Property(x => x.TimeControl)
            .HasColumnName("TimeControl");
    }
);
```

### Collection of Value Objects

```csharp
builder.OwnsMany(
    t => t.Addresses,
    a =>
    {
        a.ToTable("TournamentAddresses"); // Store in separate table
        a.HasKey(x => x.Id); // Owned entities need a key when stored separately

        a.Property(x => x.Street).HasMaxLength(200);
        a.Property(x => x.City).HasMaxLength(100);
        a.Property(x => x.PostalCode).HasMaxLength(20);
    }
);
```

## Relationship Configuration

### One-to-Many Relationship

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Configure one-to-many relationship
    builder
        .HasMany(t => t.Players)           // Tournament has many Players
        .WithOne()                          // Each Player has one Tournament (no navigation back)
        .HasForeignKey(p => p.TournamentId) // Foreign key in Player table
        .OnDelete(DeleteBehavior.Cascade);  // Delete behavior

    // Configure navigation property to use private backing field
    builder
        .Navigation(t => t.Players)
        .UsePropertyAccessMode(PropertyAccessMode.Field)
        .HasField("_players"); // Private backing field name

    // Another one-to-many example
    builder
        .HasMany(t => t.Rounds)
        .WithOne()
        .HasForeignKey(r => r.TournamentId)
        .OnDelete(DeleteBehavior.Cascade);

    builder
        .Navigation(t => t.Rounds)
        .UsePropertyAccessMode(PropertyAccessMode.Field)
        .HasField("_rounds");
}
```

### Many-to-Many Relationship

```csharp
public void Configure(EntityTypeBuilder<Player> builder)
{
    // Many-to-many using explicit join entity
    builder
        .HasMany(p => p.Tournaments)
        .WithMany(t => t.Players)
        .UsingEntity<Dictionary<string, object>>(
            "PlayerTournaments",
            j => j
                .HasOne<Tournament>()
                .WithMany()
                .HasForeignKey("TournamentId")
                .OnDelete(DeleteBehavior.Cascade),
            j => j
                .HasOne<Player>()
                .WithMany()
                .HasForeignKey("PlayerId")
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.ToTable("PlayerTournaments");
                j.HasKey("PlayerId", "TournamentId");
            }
        );
}
```

### One-to-One Relationship

```csharp
public void Configure(EntityTypeBuilder<Player> builder)
{
    builder
        .HasOne(p => p.Profile)
        .WithOne()
        .HasForeignKey<PlayerProfile>(pp => pp.PlayerId)
        .OnDelete(DeleteBehavior.Cascade);
}
```

## Advanced Configurations

### Ignoring Properties

Domain events and other transient properties should be ignored:

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Ignore domain events (they shouldn't be persisted)
    builder.Ignore(t => t.DomainEvents);

    // Ignore other computed properties
    builder.Ignore(t => t.IsCompleted);
}
```

### Table Splitting

Multiple entities sharing the same table:

```csharp
// Main entity
builder.ToTable("Tournaments");

// Split entity sharing the same table
builder.OwnsOne(t => t.AuditInfo, ai =>
{
    ai.ToTable("Tournaments"); // Same table
    ai.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
    ai.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
});
```

### Query Filters (Soft Delete)

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Add IsDeleted property
    builder.Property(t => t.IsDeleted)
        .HasDefaultValue(false);

    // Global query filter
    builder.HasQueryFilter(t => !t.IsDeleted);
}
```

### Concurrency Token

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Timestamp for optimistic concurrency
    builder.Property(t => t.RowVersion)
        .IsRowVersion();

    // Or use a regular property
    builder.Property(t => t.Version)
        .IsConcurrencyToken();
}
```

### Default Values

```csharp
public void Configure(EntityTypeBuilder<Tournament> builder)
{
    // Default value
    builder.Property(t => t.Status)
        .HasDefaultValue(TournamentStatus.Draft);

    // SQL default
    builder.Property(t => t.CreatedAt)
        .HasDefaultValueSql("GETUTCDATE()");
}
```

## Best Practices

### 1. Configuration Organization

```
Infrastructure/
  Persistence/
    Configurations/
      TournamentConfiguration.cs
      RoundConfiguration.cs
      TournamentPlayerConfiguration.cs
      OutboxMessageConfiguration.cs
      InboxMessageConfiguration.cs
    TournamentsDbContext.cs
```

### 2. Naming Conventions

- **Configuration Class**: `{EntityName}Configuration`
- **Table Name**: Plural of entity name (e.g., `Tournaments`)
- **Foreign Key**: `{ReferencedEntity}Id` (e.g., `TournamentId`)
- **Indexes**: `IX_{TableName}_{Columns}` (auto-generated or explicitly named)

### 3. Required vs Optional Properties

```csharp
// C# nullable reference types align with database nullability
public class Tournament
{
    public string Name { get; set; } = null!; // Required (non-nullable)
    public string? Description { get; set; }  // Optional (nullable)
}

// Configuration
builder.Property(t => t.Name).IsRequired();    // Required
builder.Property(t => t.Description);          // Optional (no .IsRequired())
```

### 4. String Length Constraints

Always specify max length for strings to avoid `nvarchar(max)`:

```csharp
builder.Property(t => t.Name).HasMaxLength(200);
builder.Property(t => t.Email).HasMaxLength(254); // RFC 5321 max email length
builder.Property(t => t.PhoneNumber).HasMaxLength(20);
```

### 5. Enum Configuration

Store enums as strings for readability:

```csharp
builder.Property(t => t.Status)
    .HasConversion<string>()  // Store as string instead of int
    .HasMaxLength(50);
```

### 6. Foreign Key External References

For external IDs (Auth0, etc.), use string with appropriate length:

```csharp
builder.Property(t => t.OrganizerId)
    .IsRequired()
    .HasMaxLength(450); // Auth0 User ID max length
```

### 7. Decimal Precision

Always specify precision for decimal types:

```csharp
builder.Property(t => t.EntryFee)
    .HasColumnType("decimal(18,2)"); // 18 total digits, 2 decimal places
```

### 8. Indexing Strategy

Create indexes for:
- Foreign keys (if not auto-created)
- Frequently queried columns
- Columns in WHERE clauses
- Columns in ORDER BY clauses
- Columns in JOIN conditions

```csharp
builder.HasIndex(t => t.OrganizerId);
builder.HasIndex(t => t.Status);
builder.HasIndex(t => t.StartDate);
```

### 9. Navigation Properties with Private Backing Fields

For aggregate roots, use private backing fields for collections:

```csharp
// Domain Entity
public class Tournament : Entity, IAggregateRoot
{
    private readonly List<TournamentPlayer> _players = new();

    // Public read-only access
    public IReadOnlyCollection<TournamentPlayer> Players => _players.AsReadOnly();

    // Methods to modify the collection
    public Result AddPlayer(string playerId, string playerName, int rating)
    {
        // Business logic here
        _players.Add(TournamentPlayer.Create(Id, playerId, playerName, rating));
        return Result.Success();
    }
}

// Configuration
builder
    .Navigation(t => t.Players)
    .UsePropertyAccessMode(PropertyAccessMode.Field)
    .HasField("_players");
```

### 10. Separate Configurations Per Entity

Each entity should have its own configuration class, even if small:

```csharp
// ✅ Good - Separate files
TournamentConfiguration.cs
RoundConfiguration.cs
TournamentPlayerConfiguration.cs

// ❌ Bad - Multiple entities in one configuration
AllEntitiesConfiguration.cs
```

## Common Patterns

### Pattern 1: Aggregate Root Configuration

```csharp
public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        // Table and key
        builder.ToTable("Tournaments");
        builder.HasKey(t => t.Id);

        // Required properties
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.OrganizerId).IsRequired().HasMaxLength(450);
        builder.Property(t => t.StartDate).IsRequired();

        // Optional properties
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Location).HasMaxLength(200);
        builder.Property(t => t.EndDate);

        // Enum as string
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);

        // Value object
        builder.OwnsOne(t => t.Settings, s =>
        {
            // Configure value object properties
        });

        // Child entities
        builder
            .HasMany(t => t.Players)
            .WithOne()
            .HasForeignKey(p => p.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(t => t.Players)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_players");

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);

        // Indexes
        builder.HasIndex(t => t.OrganizerId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.StartDate);
    }
}
```

### Pattern 2: Child Entity Configuration

```csharp
public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");
        builder.HasKey(r => r.Id);

        // Foreign key to parent
        builder.Property(r => r.TournamentId).IsRequired();

        // Properties
        builder.Property(r => r.RoundNumber).IsRequired();
        builder.Property(r => r.StartDate).IsRequired();
        builder.Property(r => r.EndDate);

        // Enum as string
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);

        // Indexes
        builder.HasIndex(r => r.TournamentId);
        builder.HasIndex(r => new { r.TournamentId, r.RoundNumber });
    }
}
```

### Pattern 3: Outbox/Inbox Message Configuration

```csharp
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Content)
            .IsRequired();

        builder.Property(o => o.OccurredOnUtc)
            .IsRequired();

        builder.Property(o => o.ProcessedOnUtc);

        builder.Property(o => o.Error)
            .HasMaxLength(2000);

        // Indexes for efficient querying
        builder.HasIndex(o => o.OccurredOnUtc);
        builder.HasIndex(o => o.ProcessedOnUtc);
    }
}
```

## Migration Workflow

After creating or modifying entity configurations:

1. **Create Migration**:
```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext
```

2. **Review Migration**:
Check the generated migration file to ensure it matches your intentions.

3. **Apply Migration**:
```bash
dotnet ef database update \
  --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure \
  --startup-project src/ChessTournaments.API \
  --context TournamentsDbContext
```

For more details, see [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md).

## References

- [EF Core Fluent API Documentation](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Entity Type Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/entity-types)
- [Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- [Value Objects (Owned Entities)](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities)
- [Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
- [Architecture Guide](../ARCHITECTURE.md)
- [Migrations Guide](MIGRATIONS_GUIDE.md)

---

*Last Updated: January 9, 2026*
