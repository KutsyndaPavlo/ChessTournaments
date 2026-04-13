using MediatR;

namespace ChessTournaments.Shared.IntegrationEvents;

/// <summary>
/// Implementation of IIntegrationEventPublisher using MediatR for in-process event publishing
/// </summary>
public class MediatRIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatRIntegrationEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default
    )
        where TEvent : IIntegrationEvent
    {
        await _publisher.Publish(@event, cancellationToken);
    }
}
