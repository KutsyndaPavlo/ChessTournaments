using ChessTournaments.Shared.IntegrationEvents;

namespace ChessTournaments.Modules.Matches.IntegrationEvents;

/// <summary>
/// Published when a match is completed in the Matches module
/// </summary>
public record MatchCompletedIntegrationEvent(
    Guid MatchId,
    Guid TournamentId,
    Guid RoundId,
    string WhitePlayerId,
    string BlackPlayerId,
    int Result, // GameResult enum value (0=Ongoing, 1=WhiteWins, 2=BlackWins, 3=Draw, 4=Forfeit)
    DateTime CompletedAt
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
