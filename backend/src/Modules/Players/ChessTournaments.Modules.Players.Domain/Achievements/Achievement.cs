using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Players.Domain.Achievements;

/// <summary>
/// Represents a tournament achievement for a player
/// </summary>
public class Achievement : BaseEntity
{
    public Guid PlayerId { get; private set; }
    public Guid TournamentId { get; private set; }
    public string TournamentName { get; private set; }
    public int Position { get; private set; } // 1, 2, or 3
    public decimal Score { get; private set; }
    public DateTime AchievedAt { get; private set; }

    private Achievement() { }

    private Achievement(
        Guid playerId,
        Guid tournamentId,
        string tournamentName,
        int position,
        decimal score,
        DateTime achievedAt
    )
    {
        PlayerId = playerId;
        TournamentId = tournamentId;
        TournamentName = tournamentName;
        Position = position;
        Score = score;
        AchievedAt = achievedAt;
    }

    public static Result<Achievement> Create(
        Guid playerId,
        Guid tournamentId,
        string tournamentName,
        int position,
        decimal score,
        DateTime achievedAt
    )
    {
        if (playerId == Guid.Empty)
            return Result.Failure<Achievement>("Player ID is required");

        if (tournamentId == Guid.Empty)
            return Result.Failure<Achievement>("Tournament ID is required");

        if (string.IsNullOrWhiteSpace(tournamentName))
            return Result.Failure<Achievement>("Tournament name is required");

        if (position < 1 || position > 3)
            return Result.Failure<Achievement>("Position must be 1, 2, or 3");

        if (score < 0)
            return Result.Failure<Achievement>("Score cannot be negative");

        var achievement = new Achievement(
            playerId,
            tournamentId,
            tournamentName,
            position,
            score,
            achievedAt
        );

        return Result.Success(achievement);
    }

    public string GetMedalEmoji() =>
        Position switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => "",
        };

    public string GetPositionText() =>
        Position switch
        {
            1 => "1st Place",
            2 => "2nd Place",
            3 => "3rd Place",
            _ => $"{Position}th Place",
        };
}
