using ChessTournaments.Modules.Tournaments.Domain.Common;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;

public class TournamentPlayer : BaseEntity
{
    public Guid TournamentId { get; private set; }
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }
    public int? Rating { get; private set; }
    public Score TotalScore { get; private set; }
    public int GamesPlayed { get; private set; }

    private TournamentPlayer()
    {
        // For EF
    }

    private TournamentPlayer(
        Guid tournamentId,
        string playerId,
        string playerName,
        int? rating = null
    )
    {
        TournamentId = tournamentId;
        PlayerId = playerId;
        PlayerName = playerName;
        Rating = rating;
        TotalScore = new Score(0);
        GamesPlayed = 0;
    }

    public static Result<TournamentPlayer> Create(
        Guid tournamentId,
        string playerId,
        string playerName,
        int? rating = null
    )
    {
        if (tournamentId == Guid.Empty)
            return Result.Failure<TournamentPlayer>(
                DomainErrors.TournamentPlayer.TournamentIdRequired.Message
            );

        if (string.IsNullOrWhiteSpace(playerId))
            return Result.Failure<TournamentPlayer>(
                DomainErrors.TournamentPlayer.PlayerIdRequired.Message
            );

        if (string.IsNullOrWhiteSpace(playerName))
            return Result.Failure<TournamentPlayer>(
                DomainErrors.TournamentPlayer.PlayerNameRequired.Message
            );

        var player = new TournamentPlayer(tournamentId, playerId, playerName, rating);
        return Result.Success(player);
    }

    public void UpdateScore(Score points)
    {
        TotalScore += points;
        GamesPlayed++;
        MarkAsUpdated();
    }
}
