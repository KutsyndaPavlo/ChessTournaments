using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.Common;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.RejectTournamentRequest;

public class RejectTournamentRequestCommandHandler
    : IRequestHandler<RejectTournamentRequestCommand, Result<TournamentRequestDto>>
{
    private readonly ITournamentRequestRepository _repository;

    public RejectTournamentRequestCommandHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TournamentRequestDto>> Handle(
        RejectTournamentRequestCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournamentRequest = await _repository.GetByIdAsync(
            request.RequestId,
            cancellationToken
        );

        if (tournamentRequest == null)
            return Result.Failure<TournamentRequestDto>(
                DomainErrors.TournamentRequest.NotFound.Message
            );

        var rejectResult = tournamentRequest.Reject(request.AdminId, request.RejectionReason);

        if (rejectResult.IsFailure)
            return Result.Failure<TournamentRequestDto>(rejectResult.Error);

        await _repository.UpdateAsync(tournamentRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new TournamentRequestDto
            {
                Id = tournamentRequest.Id,
                TournamentId = tournamentRequest.TournamentId,
                RequestedBy = tournamentRequest.RequestedBy,
                Status = tournamentRequest.Status,
                ReviewedBy = tournamentRequest.ReviewedBy,
                ReviewedAt = tournamentRequest.ReviewedAt,
                RejectionReason = tournamentRequest.RejectionReason,
                CreatedAt = tournamentRequest.CreatedAt,
            }
        );
    }
}
