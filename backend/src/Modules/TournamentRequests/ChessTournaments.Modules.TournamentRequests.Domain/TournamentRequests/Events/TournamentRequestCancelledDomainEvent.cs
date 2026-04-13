using ChessTournaments.Modules.TournamentRequests.Domain.Common;

namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests.Events;

public sealed class TournamentRequestCancelledDomainEvent : DomainEventBase
{
    public Guid TournamentRequestId { get; }
    public string CancelledBy { get; }

    public TournamentRequestCancelledDomainEvent(Guid tournamentRequestId, string cancelledBy)
    {
        TournamentRequestId = tournamentRequestId;
        CancelledBy = cancelledBy;
    }
}
