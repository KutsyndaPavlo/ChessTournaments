using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.CreateTournamentRequest;

public class CreateTournamentRequestCommandHandler
    : IRequestHandler<CreateTournamentRequestCommand, Result<TournamentRequestDto>>
{
    private readonly ITournamentRequestRepository _repository;

    public CreateTournamentRequestCommandHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TournamentRequestDto>> Handle(
        CreateTournamentRequestCommand request,
        CancellationToken cancellationToken
    )
    {
        var requestResult = TournamentRequest.Create(request.TournamentId, request.RequestedBy);

        if (requestResult.IsFailure)
            return Result.Failure<TournamentRequestDto>(requestResult.Error);

        var tournamentRequest = requestResult.Value;

        await _repository.AddAsync(tournamentRequest, cancellationToken);
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
