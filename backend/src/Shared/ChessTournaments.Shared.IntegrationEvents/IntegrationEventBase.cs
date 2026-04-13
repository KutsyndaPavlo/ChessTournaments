namespace ChessTournaments.Shared.IntegrationEvents;

public abstract class IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    protected IntegrationEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }
}
