using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.AddMatchReference;

public class AddMatchReferenceCommandHandler : IRequestHandler<AddMatchReferenceCommand, Result>
{
    private readonly ITournamentRepository _tournamentRepository;

    public AddMatchReferenceCommandHandler(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    public async Task<Result> Handle(
        AddMatchReferenceCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _tournamentRepository.GetByIdAsync(
            request.TournamentId,
            cancellationToken
        );

        if (tournament == null)
        {
            return Result.Failure(DomainErrors.Tournament.NotFound.Message);
        }

        // Find the round
        var round = tournament.Rounds.FirstOrDefault(r => r.Id == request.RoundId);
        if (round == null)
        {
            return Result.Failure("Round not found");
        }

        // Add match reference to round
        var addRefResult = round.AddMatchReference(request.MatchId, request.BoardNumber);
        if (addRefResult.IsFailure)
        {
            return Result.Failure(addRefResult.Error);
        }

        await _tournamentRepository.UpdateAsync(tournament, cancellationToken);
        await _tournamentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
