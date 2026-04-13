using System.Text.Json;
using ChessTournaments.Shared.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Shared.IntegrationEvents.Outbox;

/// <summary>
/// Publishes integration events to the outbox table for reliable delivery
/// </summary>
public class OutboxMessagePublisher : IOutboxMessagePublisher
{
    private readonly DbContext _dbContext;
    private readonly ILogger<OutboxMessagePublisher>? _logger;

    public OutboxMessagePublisher(
        DbContext dbContext,
        ILogger<OutboxMessagePublisher>? logger = null
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    )
    {
        var outboxMessage = new OutboxMessage(
            integrationEvent.EventId,
            integrationEvent.GetType().AssemblyQualifiedName!,
            JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            integrationEvent.OccurredAt
        );

        _logger?.LogInformation(
            "Publishing integration event {EventType} with ID {EventId} to outbox. DbContext: {DbContextType}, Instance: {DbContextHashCode}",
            integrationEvent.GetType().Name,
            integrationEvent.EventId,
            _dbContext.GetType().Name,
            _dbContext.GetHashCode()
        );

        await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage, cancellationToken);

        var entry = _dbContext.Entry(outboxMessage);
        _logger?.LogInformation(
            "OutboxMessage {MessageId} added with state: {State}. Pending changes: {ChangeCount}",
            outboxMessage.Id,
            entry.State,
            _dbContext.ChangeTracker.Entries().Count()
        );
    }
}
