# Outbox/Inbox Pattern Implementation

This document describes the implementation of the Outbox and Inbox patterns for reliable inter-module messaging in the Chess Tournaments application.

## Overview

The Outbox and Inbox patterns ensure reliable message delivery between modules in a modular monolith or microservices architecture:

- **Outbox Pattern**: Ensures that integration events are reliably published, even in case of failures
- **Inbox Pattern**: Ensures that integration events are processed exactly once (idempotency)

## Architecture

### Outbox Pattern Flow

1. When a domain event occurs, an integration event is created
2. Instead of publishing immediately, the event is stored in an `OutboxMessages` table within the same database transaction
3. A background service (`OutboxMessageProcessor`) periodically polls the outbox table
4. Unprocessed messages are published to MediatR for cross-module delivery
5. Successfully published messages are marked as processed

### Inbox Pattern Flow

1. When a module receives an integration event, it first checks the `InboxMessages` table
2. If the message ID already exists, it's skipped (idempotency protection)
3. If new, the message is added to the inbox table and processed
4. After successful processing, the message is marked as processed
5. Failed processing attempts are logged with error details

## Components

### Domain Models

#### OutboxMessage
Located in: `ChessTournaments.Shared.Domain/Outbox/OutboxMessage.cs`

```csharp
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
}
```

#### InboxMessage
Located in: `ChessTournaments.Shared.Domain/Inbox/InboxMessage.cs`

```csharp
public class InboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
}
```

### Infrastructure Services

#### IOutboxMessagePublisher
Interface for publishing events to the outbox table.

```csharp
public interface IOutboxMessagePublisher
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
```

#### OutboxMessageProcessor<TDbContext>
Generic background service that processes outbox messages every 10 seconds for a specific DbContext.

- Fetches up to 20 unprocessed messages from the DbContext
- Deserializes and publishes them via MediatR
- Marks messages as processed or failed
- Logs all activities
- Uses the specific DbContext type to avoid service resolution issues

#### IInboxMessageHandler
Interface for handling integration events with idempotency.

```csharp
public interface IInboxMessageHandler
{
    Task<bool> TryProcessAsync(
        Guid messageId,
        string messageType,
        string content,
        CancellationToken cancellationToken = default
    );
}
```

#### OutboxIntegrationEventPublisher
Implements `IIntegrationEventPublisher` using the Outbox pattern.

```csharp
public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        await _outboxPublisher.PublishAsync(@event, cancellationToken);
    }
}
```

## Configuration

### Module Setup (Example: Tournaments Module)

1. **Add Outbox/Inbox tables to DbContext**

```csharp
public class TournamentsDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    // ... rest of DbContext
}
```

2. **Configure EF Core mappings**

Create configuration classes in `Persistence/Configurations/`:
- `OutboxMessageConfiguration.cs`
- `InboxMessageConfiguration.cs`

3. **Register services in module**

```csharp
public static IServiceCollection AddTournamentsModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing services

    // Outbox/Inbox patterns for reliable messaging
    services.AddOutboxInboxPatterns<TournamentsDbContext>();

    // Use Outbox-based integration event publisher
    services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();

    return services;
}
```

### Extension Methods

Located in: `ChessTournaments.Shared.IntegrationEvents/Extensions/OutboxInboxExtensions.cs`

```csharp
// Add both patterns
services.AddOutboxInboxPatterns<TDbContext>();

// Or add individually
services.AddOutboxPattern<TDbContext>();
services.AddInboxPattern<TDbContext>();
```

## Database Migrations

After adding Outbox/Inbox to a module, create a migration:

```bash
cd backend
dotnet ef migrations add AddOutboxInbox --project src/Modules/Tournaments/ChessTournaments.Modules.Tournaments.Infrastructure --startup-project src/ChessTournaments.API
```

The migration will create two tables:
- `Tournaments.OutboxMessages`
- `Tournaments.InboxMessages`

Both tables include:
- Primary key on `Id`
- Indexes on `ProcessedOnUtc` and `OccurredOnUtc` for efficient querying

## Usage Example

### Publishing an Event (Outbox)

```csharp
public class Tournament : Entity
{
    public void CompleteTournament()
    {
        // ... business logic

        // Raise domain event
        AddDomainEvent(new TournamentCompletedDomainEvent(Id, Name));
    }
}

// Domain event handler
public class TournamentCompletedDomainEventHandler : INotificationHandler<TournamentCompletedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public async Task Handle(TournamentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Create integration event
        var integrationEvent = new TournamentCompletedIntegrationEvent(
            notification.TournamentId,
            notification.TournamentName
        );

        // Publish to outbox (not immediate publish)
        await _publisher.PublishAsync(integrationEvent, cancellationToken);

        // Event will be published by OutboxMessageProcessor background service
    }
}
```

### Processing an Event (Inbox)

The inbox pattern is automatically applied when using `InboxMessageHandler`. Integration event handlers work normally:

```csharp
public class TournamentCompletedIntegrationEventHandler
    : INotificationHandler<TournamentCompletedIntegrationEvent>
{
    public async Task Handle(TournamentCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Process the event
        // Idempotency is guaranteed by the Inbox pattern
    }
}
```

## Benefits

1. **Reliability**: Events are persisted before publishing, surviving crashes
2. **Idempotency**: Duplicate events are automatically detected and skipped
3. **Transactionality**: Events are stored in the same transaction as business data
4. **Observability**: All events are tracked with timestamps and error logging
5. **Resilience**: Failed events can be retried automatically
6. **Ordering**: Events are processed in the order they occurred

## Monitoring

### Checking Outbox Status

```sql
-- Unprocessed outbox messages
SELECT * FROM Tournaments.OutboxMessages
WHERE ProcessedOnUtc IS NULL
ORDER BY OccurredOnUtc;

-- Failed outbox messages
SELECT * FROM Tournaments.OutboxMessages
WHERE Error IS NOT NULL;
```

### Checking Inbox Status

```sql
-- All received messages
SELECT * FROM Tournaments.InboxMessages
ORDER BY OccurredOnUtc DESC;

-- Failed inbox messages
SELECT * FROM Tournaments.InboxMessages
WHERE Error IS NOT NULL;
```

## Configuration Options

### Background Service Interval

Currently set to 10 seconds in `OutboxMessageProcessor`. Adjust as needed:

```csharp
private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
```

### Batch Size

Currently processes 20 messages per batch. Adjust in `OutboxMessageProcessor`:

```csharp
.Take(20)
```

## Future Enhancements

1. Add retry logic with exponential backoff for failed messages
2. Implement dead letter queue for permanently failed messages
3. Add metrics and health checks for background processor
4. Support for message priority
5. Automatic cleanup of old processed messages
6. Support for distributed scenarios (if moving to microservices)

## References

- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Inbox Pattern](https://microservices.io/patterns/communication-style/idempotent-consumer.html)
- [CAP Theorem and Eventual Consistency](https://en.wikipedia.org/wiki/CAP_theorem)
