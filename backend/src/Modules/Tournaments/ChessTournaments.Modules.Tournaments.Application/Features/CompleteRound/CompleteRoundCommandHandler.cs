using ChessTournaments.Modules.Tournaments.Application.Features.CompleteTournament;
using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CompleteRound;

public class CompleteRoundCommandHandler : IRequestHandler<CompleteRoundCommand, Result>
{
    private readonly ITournamentRepository _repository;
    private readonly ISender _sender;

    public CompleteRoundCommandHandler(ITournamentRepository repository, ISender sender)
    {
        _repository = repository;
        _sender = sender;
    }

    public async Task<Result> Handle(
        CompleteRoundCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournament = await _repository.GetByIdWithPlayersAndRoundsAsync(
            request.TournamentId,
            cancellationToken
        );

        if (tournament == null)
            return Result.Failure(DomainErrors.Tournament.NotFound.Message);

        var round = tournament.Rounds.FirstOrDefault(r => r.Id == request.RoundId);
        if (round == null)
            return Result.Failure("Round not found");

        var result = round.Complete();

        if (result.IsFailure)
            return Result.Failure(result.Error);

        await _repository.UpdateAsync(tournament, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Check if this was the last round of the tournament
        var completedRoundsCount = tournament.Rounds.Count(r => r.IsCompleted);
        if (completedRoundsCount >= tournament.Settings.NumberOfRounds)
        {
            // Trigger command to complete the tournament
            var completeTournamentCommand = new CompleteTournamentCommand(request.TournamentId);
            await _sender.Send(completeTournamentCommand, cancellationToken);
        }

        return Result.Success();
    }
}
