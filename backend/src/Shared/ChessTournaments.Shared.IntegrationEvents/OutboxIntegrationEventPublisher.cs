using ChessTournaments.Shared.IntegrationEvents.Outbox;

namespace ChessTournaments.Shared.IntegrationEvents;

/// <summary>
/// Integration event publisher using the Outbox pattern for reliable delivery
/// </summary>
public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public OutboxIntegrationEventPublisher(IOutboxMessagePublisher outboxPublisher)
    {
        _outboxPublisher = outboxPublisher;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        await _outboxPublisher.PublishAsync(@event, cancellationToken);
    }
}
