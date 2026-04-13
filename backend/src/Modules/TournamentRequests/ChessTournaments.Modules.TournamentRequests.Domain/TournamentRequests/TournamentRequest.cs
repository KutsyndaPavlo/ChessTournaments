using ChessTournaments.Modules.TournamentRequests.Domain.Common;
using ChessTournaments.Modules.TournamentRequests.Domain.Enums;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests.Events;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;

public class TournamentRequest : BaseEntity
{
    public Guid TournamentId { get; private set; }
    public string RequestedBy { get; private set; }
    public RequestStatus Status { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private TournamentRequest() { } // EF Core

    private TournamentRequest(Guid tournamentId, string requestedBy)
    {
        Id = Guid.NewGuid();
        TournamentId = tournamentId;
        RequestedBy = requestedBy;
        Status = RequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TournamentRequestCreatedDomainEvent(Id, requestedBy));
    }

    public static Result<TournamentRequest> Create(Guid tournamentId, string requestedBy)
    {
        if (tournamentId == Guid.Empty)
            return Result.Failure<TournamentRequest>("Tournament ID cannot be empty");

        if (string.IsNullOrWhiteSpace(requestedBy))
            return Result.Failure<TournamentRequest>("RequestedBy cannot be empty");

        var request = new TournamentRequest(tournamentId, requestedBy);

        return Result.Success(request);
    }

    public Result Approve(string adminId)
    {
        if (Status != RequestStatus.Pending)
            return Result.Failure(DomainErrors.TournamentRequest.CannotApproveNonPending.Message);

        Status = RequestStatus.Approved;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        MarkAsUpdated();

        AddDomainEvent(
            new TournamentRequestApprovedDomainEvent(Id, adminId, TournamentId, RequestedBy)
        );

        return Result.Success();
    }

    public Result Reject(string adminId, string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result.Failure(DomainErrors.TournamentRequest.RejectionReasonRequired.Message);

        if (Status != RequestStatus.Pending)
            return Result.Failure(DomainErrors.TournamentRequest.CannotRejectNonPending.Message);

        Status = RequestStatus.Rejected;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = rejectionReason;
        MarkAsUpdated();

        AddDomainEvent(new TournamentRequestRejectedDomainEvent(Id, adminId, rejectionReason));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != RequestStatus.Pending)
            return Result.Failure(DomainErrors.TournamentRequest.CannotCancelNonPending.Message);

        Status = RequestStatus.Cancelled;
        MarkAsUpdated();

        AddDomainEvent(new TournamentRequestCancelledDomainEvent(Id, RequestedBy));

        return Result.Success();
    }
}
