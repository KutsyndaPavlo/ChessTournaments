using ChessTournaments.Modules.TournamentRequests.Domain.Common;

namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests.Events;

public sealed class TournamentRequestRejectedDomainEvent : DomainEventBase
{
    public Guid TournamentRequestId { get; }
    public string RejectedBy { get; }
    public string RejectionReason { get; }

    public TournamentRequestRejectedDomainEvent(
        Guid tournamentRequestId,
        string rejectedBy,
        string rejectionReason
    )
    {
        TournamentRequestId = tournamentRequestId;
        RejectedBy = rejectedBy;
        RejectionReason = rejectionReason;
    }
}
