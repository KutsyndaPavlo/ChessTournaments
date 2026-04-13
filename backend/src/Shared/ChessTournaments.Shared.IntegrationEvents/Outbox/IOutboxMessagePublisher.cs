namespace ChessTournaments.Shared.IntegrationEvents.Outbox;

/// <summary>
/// Publishes integration events using the Outbox pattern
/// </summary>
public interface IOutboxMessagePublisher
{
    Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default
    );
}
