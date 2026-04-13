using ChessTournaments.Shared.Domain.Events;

namespace ChessTournaments.Modules.Tournaments.Domain.Shared;

public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
