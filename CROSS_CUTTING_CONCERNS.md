# Cross-Cutting Concerns

This document describes the cross-cutting concerns implemented in the Chess Tournaments application. These are aspects of the application that affect multiple modules and layers, providing consistent functionality across the entire system.

## Table of Contents

- [Overview](#overview)
- [Shared Infrastructure](#shared-infrastructure)
- [Exception Handling](#exception-handling)
- [Domain Events](#domain-events)
- [Pagination](#pagination)
- [Soft Deletes](#soft-deletes)
- [Optimistic Concurrency](#optimistic-concurrency)
- [Reliable Messaging](#reliable-messaging)
- [Logging](#logging)
- [Authentication & Authorization](#authentication--authorization)
- [Validation](#validation)
- [HTTP Pipeline](#http-pipeline)

## Overview

Cross-cutting concerns are implemented using a combination of:
- **Shared base classes** in `ChessTournaments.Shared.Domain`
- **Middleware components** in the API layer
- **Background services** for asynchronous processing
- **Infrastructure patterns** like Repository and Unit of Work

All modules leverage these shared components to ensure consistent behavior across the application.

## Shared Infrastructure

### Entity Base Class

Location: `backend/src/Shared/ChessTournaments.Shared.Domain/Entities/Entity.cs`

All domain entities inherit from the shared `Entity` base class, which provides:

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Domain events support
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    // Soft delete methods
    public void Delete();
    public void Restore();

    // Domain event methods
    protected void AddDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();

    // Update tracking
    protected void MarkAsUpdated();
}
```

**Benefits:**
- Consistent identity management across all entities
- Built-in audit trail (CreatedAt, UpdatedAt)
- Soft delete support
- Optimistic concurrency control
- Domain events infrastructure

**Usage in Modules:**

Modules use type aliases to avoid naming conflicts with `CSharpFunctionalExtensions.Entity`:

```csharp
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

public class Tournament : BaseEntity
{
    // Entity implementation
}
```

## Exception Handling

Location: `backend/src/ChessTournaments.API/Middleware/GlobalExceptionMiddleware.cs`

### Global Exception Middleware

Centralized exception handling for all API requests:

```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

**Exception Mapping:**

| Exception Type | HTTP Status Code | Response Message |
|---------------|------------------|------------------|
| `DbUpdateConcurrencyException` | 409 Conflict | "The resource was modified by another user..." |
| `DbUpdateException` | 400 Bad Request | "A database error occurred..." |
| `UnauthorizedAccessException` | 403 Forbidden | "You do not have permission..." |
| `ArgumentException` | 400 Bad Request | Exception message (dev) / Generic (prod) |
| `InvalidOperationException` | 400 Bad Request | Exception message (dev) / Generic (prod) |
| All others | 500 Internal Server Error | Exception message (dev) / Generic (prod) |

**Features:**
- **Correlation ID tracking**: Uses `Activity.Current?.Id` or `HttpContext.TraceIdentifier`
- **Environment-aware messages**: Detailed in development, generic in production
- **Structured error responses**: Consistent JSON error format
- **Comprehensive logging**: All exceptions logged with context

**Registration:**

```csharp
// In WebApplicationExtensions.cs
app.UseMiddleware<GlobalExceptionMiddleware>(); // Must be first
```

## Domain Events

Location: `backend/src/Shared/ChessTournaments.Shared.Domain/Events/`

### IDomainEvent Interface

```csharp
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
```

**Key Points:**
- Implements `INotification` from MediatR for in-process pub/sub
- All domain events must implement this interface
- Each module has a `DomainEventBase` class that implements `IDomainEvent`

### Domain Event Flow

1. **Entity raises event:**
   ```csharp
   AddDomainEvent(new TournamentCreatedDomainEvent(Id, Name, OrganizerId));
   ```

2. **DbContext collects events before save:**
   ```csharp
   var domainEvents = ChangeTracker
       .Entries<Entity>()
       .SelectMany(e => e.Entity.DomainEvents)
       .ToList();
   ```

3. **Clear events and save:**
   ```csharp
   entities.ForEach(e => e.Entity.ClearDomainEvents());
   var result = await base.SaveChangesAsync(cancellationToken);
   ```

4. **Publish events after successful save:**
   ```csharp
   foreach (var domainEvent in domainEvents)
   {
       await _publisher.Publish(domainEvent, cancellationToken);
   }
   ```

**Benefits:**
- Events only published after successful database save
- Prevents inconsistent state
- Supports multiple handlers per event
- In-process communication (same transaction context)

### Example: PlayersDbContext

```csharp
public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    var domainEvents = ChangeTracker
        .Entries<Entity>()
        .Select(e => e.Entity)
        .Where(e => e.DomainEvents.Any())
        .SelectMany(e =>
        {
            var events = e.DomainEvents.ToList();
            e.ClearDomainEvents();
            return events;
        })
        .ToList();

    var result = await base.SaveChangesAsync(cancellationToken);

    foreach (var domainEvent in domainEvents)
    {
        await _publisher.Publish(domainEvent, cancellationToken);
    }

    return result;
}
```

## Pagination

Location: `backend/src/Shared/ChessTournaments.Shared.Infrastructure/Pagination/PaginatedResult.cs`

### PaginatedResult<T>

Generic pagination wrapper for query results:

```csharp
public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

### Usage Examples

**Synchronous creation:**
```csharp
var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
var totalCount = query.Count();
var result = PaginatedResult<PlayerDto>.Create(items, totalCount, pageNumber, pageSize);
```

**Asynchronous creation:**
```csharp
var result = await PaginatedResult<PlayerDto>.CreateAsync(
    query,
    pageNumber: 1,
    pageSize: 20,
    cancellationToken
);
```

**Response structure:**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 157,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Benefits:**
- Consistent pagination across all endpoints
- Navigation metadata (HasNextPage, HasPreviousPage)
- Efficient database queries (only fetches required page)
- Supports both sync and async scenarios

## Soft Deletes

### Implementation

Soft deletes are implemented in the shared `Entity` base class:

```csharp
public bool IsDeleted { get; private set; }
public DateTime? DeletedAt { get; private set; }

public void Delete()
{
    IsDeleted = true;
    DeletedAt = DateTime.UtcNow;
    MarkAsUpdated();
}

public void Restore()
{
    IsDeleted = false;
    DeletedAt = null;
    MarkAsUpdated();
}
```

### Query Filters

**Recommended: Configure global query filters in DbContext:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply to all entities that inherit from Entity
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
        }
    }
}

private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
{
    var parameter = Expression.Parameter(entityType, "e");
    var property = Expression.Property(parameter, nameof(Entity.IsDeleted));
    var filter = Expression.Lambda(
        Expression.Equal(property, Expression.Constant(false)),
        parameter
    );
    return filter;
}
```

**Manual filtering (if global filters not configured):**
```csharp
var activePlayers = await context.Players
    .Where(p => !p.IsDeleted)
    .ToListAsync();
```

**Include deleted entities when needed:**
```csharp
var allPlayers = await context.Players
    .IgnoreQueryFilters()
    .ToListAsync();
```

**Benefits:**
- Data preservation for audit and recovery
- Maintains referential integrity
- Can be restored if deleted by mistake
- Transparent to application code (with query filters)

## Optimistic Concurrency

### Implementation

Optimistic concurrency is implemented using row versioning:

```csharp
[Timestamp]
public byte[]? RowVersion { get; set; }
```

### How It Works

1. **Entity fetched from database:**
   ```csharp
   var tournament = await context.Tournaments.FindAsync(id);
   // RowVersion is loaded with current value
   ```

2. **Entity modified:**
   ```csharp
   tournament.UpdateDetails(name, description, location);
   ```

3. **Save changes:**
   ```csharp
   await context.SaveChangesAsync();
   // EF Core checks if RowVersion matches database value
   ```

4. **Concurrent modification detected:**
   ```csharp
   catch (DbUpdateConcurrencyException ex)
   {
       // Handle conflict - throw 409 Conflict via GlobalExceptionMiddleware
   }
   ```

### EF Core Configuration

The `[Timestamp]` attribute automatically configures EF Core to:
- Generate a new value on insert
- Generate a new value on update
- Check the current value matches when updating
- Throw `DbUpdateConcurrencyException` on mismatch

**Manual configuration (if needed):**
```csharp
entity.Property(e => e.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

**Benefits:**
- Prevents lost updates
- No database locks required
- Works across stateless HTTP requests
- Automatic handling by EF Core

## Reliable Messaging

### Outbox Pattern

Ensures integration events are published reliably:

1. **Events saved in outbox table** (same transaction as business data)
2. **Background service processes outbox** every 10 seconds
3. **Events published to message bus**
4. **Processed messages marked as sent**

### Inbox Pattern

Ensures idempotent event processing:

1. **Event received**
2. **Check inbox table** for duplicate event ID
3. **If not processed:** Process event and save to inbox
4. **If already processed:** Ignore (idempotency)

**See:** [Outbox/Inbox Pattern Guide](backend/OUTBOX_INBOX_PATTERN.md) for detailed implementation.

## Logging

### Serilog Configuration

Structured logging with Seq integration:

```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ChessTournaments")
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341");
});
```

### Logging Levels

- **Verbose**: Detailed trace information
- **Debug**: Internal system events
- **Information**: General informational messages
- **Warning**: Abnormal or unexpected events
- **Error**: Errors and exceptions
- **Fatal**: Critical failures

### Best Practices

**Use structured logging:**
```csharp
_logger.LogInformation(
    "Tournament {TournamentId} created by {OrganizerId}",
    tournamentId,
    organizerId
);
```

**Log correlation IDs:**
```csharp
var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
_logger.LogError(
    exception,
    "An unhandled exception occurred. CorrelationId: {CorrelationId}",
    correlationId
);
```

**Access logs in Seq:**
- Navigate to http://localhost:5341
- Query logs using structured properties
- Create dashboards and alerts

## Authentication & Authorization

### OpenIddict Integration

The application uses OpenIddict for OAuth 2.0 / OpenID Connect authentication:

**Identity Server:** `backend/src/ChessTournaments.Identity/`
**API Authentication:** Bearer token validation

### Configuration

```csharp
services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("http://localhost:5001/");
        options.AddAudiences("chess-tournaments-api");
        options.UseIntrospection()
            .SetClientId("api")
            .SetClientSecret("api-secret");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
```

### Authorization Policies

**Public endpoints:**
```csharp
app.MapHealthChecks("/health").AllowAnonymous();
app.MapOpenApi().AllowAnonymous();
```

**Protected endpoints (default):**
All Carter endpoints require authentication unless explicitly marked as `AllowAnonymous`.

**Custom policies (future):**
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("OrganizerOnly", policy =>
        policy.RequireRole("Organizer", "Admin"));
});
```

## Validation

### FluentValidation

All commands and queries use FluentValidation for input validation:

**Example validator:**
```csharp
public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
    public CreateTournamentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tournament name is required")
            .MaximumLength(200);

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Start date must be in the future");

        RuleFor(x => x.MaxPlayers)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);
    }
}
```

**Registration:**
```csharp
services.AddValidatorsFromAssembly(typeof(CreateTournamentCommand).Assembly);
```

**MediatR pipeline behavior:**
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

**Validation flow:**
1. Request received
2. Validator executes (via MediatR pipeline)
3. If invalid: Returns validation errors (400 Bad Request)
4. If valid: Continues to handler

## HTTP Pipeline

### Middleware Order

The middleware pipeline is configured in a specific order for optimal functionality:

```csharp
public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
{
    // 1. Global exception handling - must be first
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 2. HTTPS redirection
    app.UseHttpsRedirection();

    // 3. Routing
    app.UseRouting();

    // 4. CORS (if configured)
    app.UseCors();

    // 5. Authentication
    app.UseAuthentication();

    // 6. Authorization
    app.UseAuthorization();

    // 7. Swagger UI (development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Chess Tournaments API v1");
            options.RoutePrefix = string.Empty; // Serve at root
        });
    }

    // 8. Map endpoints
    app.MapCarter();
    app.MapHealthChecks("/health").AllowAnonymous();
    app.MapHealthChecks("/health/ready").AllowAnonymous();
    app.MapOpenApi().AllowAnonymous();

    return app;
}
```

**Order is critical:**
- Exception handling must be first to catch all errors
- Authentication before authorization
- Routing before authentication/authorization
- Endpoints last

### Health Checks

**Basic health check:**
```bash
GET /health
```

**Readiness check:**
```bash
GET /health/ready
```

**Configuration:**
```csharp
services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database")
    .AddCheck("self", () => HealthCheckResult.Healthy());
```

### OpenAPI / Swagger

**OpenAPI endpoint:**
```
GET /openapi/v1.json
```

**Swagger UI:**
```
http://localhost:5000/
```

**Configuration:**
```csharp
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Chess Tournaments API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});
```

## Summary

The Chess Tournaments application implements comprehensive cross-cutting concerns to ensure:

- **Consistency**: Shared base classes and patterns across all modules
- **Reliability**: Exception handling, optimistic concurrency, reliable messaging
- **Observability**: Structured logging, correlation IDs, health checks
- **Security**: Authentication, authorization, input validation
- **Maintainability**: Clean separation of concerns, reusable infrastructure
- **Scalability**: Pagination, soft deletes, background processing

All modules leverage these shared components, reducing code duplication and ensuring consistent behavior across the application.
