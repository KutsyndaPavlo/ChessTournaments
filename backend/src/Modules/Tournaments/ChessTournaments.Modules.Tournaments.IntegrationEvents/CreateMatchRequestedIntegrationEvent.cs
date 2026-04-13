using ChessTournaments.Shared.IntegrationEvents;

namespace ChessTournaments.Modules.Tournaments.IntegrationEvents;

/// <summary>
/// Published by Tournaments module to request match creation from Matches module
/// </summary>
public record CreateMatchRequestedIntegrationEvent(
    Guid RoundId,
    Guid TournamentId,
    string WhitePlayerId,
    string BlackPlayerId,
    int BoardNumber
) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
