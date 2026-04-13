using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.Common;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.CancelTournamentRequest;

public class CancelTournamentRequestCommandHandler
    : IRequestHandler<CancelTournamentRequestCommand, Result<TournamentRequestDto>>
{
    private readonly ITournamentRequestRepository _repository;

    public CancelTournamentRequestCommandHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TournamentRequestDto>> Handle(
        CancelTournamentRequestCommand request,
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

        var cancelResult = tournamentRequest.Cancel();

        if (cancelResult.IsFailure)
            return Result.Failure<TournamentRequestDto>(cancelResult.Error);

        await _repository.UpdateAsync(tournamentRequest, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new TournamentRequestDto
            {
                Id = tournamentRequest.Id,
                TournamentId = tournamentRequest.TournamentId,
                RequestedBy = tournamentRequest.RequestedBy,
                Status = tournamentRequest.Status,
                CreatedAt = tournamentRequest.CreatedAt,
            }
        );
    }
}
