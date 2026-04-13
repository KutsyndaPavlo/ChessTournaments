using ChessTournaments.Shared.IntegrationEvents;

namespace ChessTournaments.Modules.Matches.IntegrationEvents;

/// <summary>
/// Published when a match is created in the Matches module
/// </summary>
public record MatchCreatedIntegrationEvent(
    Guid MatchId,
    Guid TournamentId,
    Guid RoundId,
    string WhitePlayerId,
    string BlackPlayerId,
    int BoardNumber
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
