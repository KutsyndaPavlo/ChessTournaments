using ChessTournaments.Modules.TournamentRequests.Domain.Common;

namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests.Events;

public sealed class TournamentRequestApprovedDomainEvent : DomainEventBase
{
    public Guid TournamentRequestId { get; }
    public string ApprovedBy { get; }
    public Guid TournamentId { get; }
    public string RequestedBy { get; }

    public TournamentRequestApprovedDomainEvent(
        Guid tournamentRequestId,
        string approvedBy,
        Guid tournamentId,
        string requestedBy
    )
    {
        TournamentRequestId = tournamentRequestId;
        ApprovedBy = approvedBy;
        TournamentId = tournamentId;
        RequestedBy = requestedBy;
    }
}
