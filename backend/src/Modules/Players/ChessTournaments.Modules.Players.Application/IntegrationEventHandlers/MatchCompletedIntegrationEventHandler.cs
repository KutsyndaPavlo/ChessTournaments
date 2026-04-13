using ChessTournaments.Modules.Matches.IntegrationEvents;
using ChessTournaments.Modules.Players.Domain.Players;
using MediatR;

namespace ChessTournaments.Modules.Players.Application.IntegrationEventHandlers;

/// <summary>
/// Handles match completion events to update player statistics
/// Updates: Matches Played, Win Rate (Wins/Losses/Draws)
/// </summary>
public class MatchCompletedIntegrationEventHandler
    : INotificationHandler<MatchCompletedIntegrationEvent>
{
    private readonly IPlayerRepository _playerRepository;

    public MatchCompletedIntegrationEventHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task Handle(
        MatchCompletedIntegrationEvent notification,
        CancellationToken cancellationToken
    )
    {
        // Get both players
        var whitePlayer = await _playerRepository.GetByUserIdAsync(
            notification.WhitePlayerId,
            cancellationToken
        );
        var blackPlayer = await _playerRepository.GetByUserIdAsync(
            notification.BlackPlayerId,
            cancellationToken
        );

        if (whitePlayer == null || blackPlayer == null)
        {
            // Players not found - they might not be registered yet
            return;
        }

        // Calculate new ratings using Elo rating system before recording results
        var (whiteNewRating, blackNewRating) = CalculateEloRatings(
            whitePlayer.Rating,
            blackPlayer.Rating,
            notification.Result
        );

        // Map result enum: 0=Ongoing, 1=WhiteWins, 2=BlackWins, 3=Draw, 4=Forfeit
        switch (notification.Result)
        {
            case 0: // White wins
                whitePlayer.RecordGameResult(won: true, draw: false);
                blackPlayer.RecordGameResult(won: false, draw: false);
                whitePlayer.UpdateRating(whiteNewRating);
                blackPlayer.UpdateRating(blackNewRating);
                break;

            case 1: // Black wins
                blackPlayer.RecordGameResult(won: true, draw: false);
                whitePlayer.RecordGameResult(won: false, draw: false);
                whitePlayer.UpdateRating(whiteNewRating);
                blackPlayer.UpdateRating(blackNewRating);
                break;

            case 2: // Draw
                whitePlayer.RecordGameResult(won: false, draw: true);
                blackPlayer.RecordGameResult(won: false, draw: true);
                whitePlayer.UpdateRating(whiteNewRating);
                blackPlayer.UpdateRating(blackNewRating);
                break;

            default:
                return;
        }

        // Save changes
        await _playerRepository.UpdateAsync(whitePlayer, cancellationToken);
        await _playerRepository.UpdateAsync(blackPlayer, cancellationToken);
        await _playerRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Calculate new Elo ratings for both players based on match result
    /// </summary>
    /// <param name="whiteRating">Current rating of white player</param>
    /// <param name="blackRating">Current rating of black player</param>
    /// <param name="result">Match result (1=WhiteWins, 2=BlackWins, 3=Draw)</param>
    /// <returns>Tuple of (white new rating, black new rating)</returns>
    private (int whiteNewRating, int blackNewRating) CalculateEloRatings(
        int whiteRating,
        int blackRating,
        int result
    )
    {
        const int kFactor = 32; // Standard K-factor for active players

        // Expected scores (probability of winning)
        var whiteExpected = 1.0 / (1.0 + Math.Pow(10, (blackRating - whiteRating) / 400.0));
        var blackExpected = 1.0 / (1.0 + Math.Pow(10, (whiteRating - blackRating) / 400.0));

        // Actual scores
        double whiteActual,
            blackActual;
        switch (result)
        {
            case 0: // White wins
                whiteActual = 1.0;
                blackActual = 0.0;
                break;
            case 1: // Black wins
                whiteActual = 0.0;
                blackActual = 1.0;
                break;
            case 2: // Draw
                whiteActual = 0.5;
                blackActual = 0.5;
                break;
            default:
                return (whiteRating, blackRating);
        }

        // Calculate new ratings
        var whiteNewRating = (int)Math.Round(whiteRating + kFactor * (whiteActual - whiteExpected));
        var blackNewRating = (int)Math.Round(blackRating + kFactor * (blackActual - blackExpected));

        return (whiteNewRating, blackNewRating);
    }
}
