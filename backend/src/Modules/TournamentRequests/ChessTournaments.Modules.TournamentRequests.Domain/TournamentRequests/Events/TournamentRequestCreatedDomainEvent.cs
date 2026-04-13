using ChessTournaments.Modules.TournamentRequests.Domain.Common;

namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests.Events;

public sealed class TournamentRequestCreatedDomainEvent : DomainEventBase
{
    public Guid TournamentRequestId { get; }
    public string RequestedBy { get; }

    public TournamentRequestCreatedDomainEvent(Guid tournamentRequestId, string requestedBy)
    {
        TournamentRequestId = tournamentRequestId;
        RequestedBy = requestedBy;
    }
}
