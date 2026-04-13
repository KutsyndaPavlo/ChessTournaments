using ChessTournaments.Shared.Domain.Enums;

namespace ChessTournaments.Modules.Matches.Application.Abstractions;

public class TournamentMatchDto
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public Guid TournamentId { get; set; }
    public string WhitePlayerId { get; set; } = string.Empty;
    public string BlackPlayerId { get; set; } = string.Empty;
    public int BoardNumber { get; set; }
    public GameResult Result { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string? Moves { get; set; }
    public List<MatchTagDto> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
