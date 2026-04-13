using ChessTournaments.Shared.IntegrationEvents;

namespace ChessTournaments.Modules.Tournaments.IntegrationEvents;

/// <summary>
/// Published when a tournament is completed
/// Contains information about the top 3 winners
/// </summary>
public record TournamentCompletedIntegrationEvent(
    Guid TournamentId,
    string TournamentName,
    DateTime CompletedAt,
    List<WinnerInfo> TopWinners
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Information about a tournament winner
/// </summary>
public record WinnerInfo(
    int Position, // 1, 2, or 3
    string PlayerId,
    string PlayerName,
    decimal Score
);
