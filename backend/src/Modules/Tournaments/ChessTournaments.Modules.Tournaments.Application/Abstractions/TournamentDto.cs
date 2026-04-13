using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;

namespace ChessTournaments.Modules.Tournaments.Application.Abstractions;

public record TournamentDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public TournamentStatus Status { get; init; }
    public string OrganizerId { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public TournamentSettingsDto Settings { get; init; } = null!;
    public int PlayerCount { get; init; }
    public int RoundCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TournamentSettingsDto
{
    public TournamentFormat Format { get; init; }
    public TimeControl TimeControl { get; init; }
    public int TimeInMinutes { get; init; }
    public int IncrementInSeconds { get; init; }
    public int NumberOfRounds { get; init; }
    public int MaxPlayers { get; init; }
    public int MinPlayers { get; init; }
    public bool AllowByes { get; init; }
    public decimal EntryFee { get; init; }
}

public record TournamentPlayerDto
{
    public Guid Id { get; init; }
    public string PlayerId { get; init; } = string.Empty;
    public string PlayerName { get; init; } = string.Empty;
    public int? Rating { get; init; }
    public decimal TotalScore { get; init; }
    public int GamesPlayed { get; init; }
}

public record RoundDto
{
    public Guid Id { get; init; }
    public Guid TournamentId { get; init; }
    public int RoundNumber { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public bool IsCompleted { get; init; }
    public int MatchCount { get; init; }
    public List<MatchDto> Matches { get; init; } = [];
}

public record MatchDto
{
    public Guid Id { get; init; }
    public Guid RoundId { get; init; }
    public Guid TournamentId { get; init; }
    public int BoardNumber { get; init; }
    public string WhitePlayerId { get; init; } = string.Empty;
    public string BlackPlayerId { get; init; } = string.Empty;
    public GameResult Result { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? Moves { get; init; }
}
