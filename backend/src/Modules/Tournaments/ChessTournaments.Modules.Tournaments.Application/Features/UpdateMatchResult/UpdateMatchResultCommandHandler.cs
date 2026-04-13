using ChessTournaments.Modules.Tournaments.Application.Features.CompleteRound;
using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.UpdateMatchResult;

public class UpdateMatchResultCommandHandler : IRequestHandler<UpdateMatchResultCommand, Result>
{
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ISender _sender;

    public UpdateMatchResultCommandHandler(
        ITournamentRepository tournamentRepository,
        ISender sender
    )
    {
        _tournamentRepository = tournamentRepository;
        _sender = sender;
    }

    public async Task<Result> Handle(
        UpdateMatchResultCommand request,
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

        // Update player scores based on match result
        var whitePlayer = tournament.Players.FirstOrDefault(p =>
            p.PlayerId == request.WhitePlayerId
        );
        var blackPlayer = tournament.Players.FirstOrDefault(p =>
            p.PlayerId == request.BlackPlayerId
        );

        if (whitePlayer == null || blackPlayer == null)
        {
            return Result.Failure("One or more players not found in tournament");
        }

        // Map int result value to Tournaments.GameResult
        var tournamentResult = (GameResult)request.Result;

        // Update scores based on result
        switch (tournamentResult)
        {
            case GameResult.WhiteWin:
                whitePlayer.UpdateScore(Domain.TournamentPlayers.Score.Win);
                blackPlayer.UpdateScore(Domain.TournamentPlayers.Score.Loss);
                break;
            case GameResult.BlackWin:
                blackPlayer.UpdateScore(Domain.TournamentPlayers.Score.Win);
                whitePlayer.UpdateScore(Domain.TournamentPlayers.Score.Loss);
                break;
            case GameResult.Draw:
                whitePlayer.UpdateScore(Domain.TournamentPlayers.Score.Draw);
                blackPlayer.UpdateScore(Domain.TournamentPlayers.Score.Draw);
                break;
            // Forfeit doesn't update scores automatically
        }

        // Mark match as completed in round
        round.MarkMatchCompleted(request.MatchId);

        await _tournamentRepository.UpdateAsync(tournament, cancellationToken);
        await _tournamentRepository.SaveChangesAsync(cancellationToken);

        // Check if all matches in the round are completed
        if (round.MatchReferences.All(m => m.IsCompleted))
        {
            // Trigger command to complete the round
            var completeRoundCommand = new CompleteRoundCommand(
                request.TournamentId,
                request.RoundId
            );
            await _sender.Send(completeRoundCommand, cancellationToken);
        }

        return Result.Success();
    }
}
