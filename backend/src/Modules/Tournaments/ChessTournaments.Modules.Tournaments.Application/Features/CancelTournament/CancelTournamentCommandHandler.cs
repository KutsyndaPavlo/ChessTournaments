using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CancelTournament;

public class CancelTournamentCommandHandler : IRequestHandler<CancelTournamentCommand, Result>
{
    private readonly ITournamentRepository _repository;

    public CancelTournamentCommandHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        CancelTournamentCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdAsync(request.TournamentId, cancellationToken);

        if (tournament == null)
            return Result.Failure(DomainErrors.Tournament.NotFound.Message);

        var result = tournament.Cancel();

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
