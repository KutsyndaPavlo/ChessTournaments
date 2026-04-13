using ChessTournaments.Shared.Domain.Events;

namespace ChessTournaments.Modules.TournamentRequests.Domain.Common;

public abstract class DomainEventBase : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }

    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
