namespace ChessTournaments.Shared.IntegrationEvents;

/// <summary>
/// Publishes integration events across module boundaries
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
