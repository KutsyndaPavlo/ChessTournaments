using MediatR;

namespace ChessTournaments.Shared.IntegrationEvents;

/// <summary>
/// Published when a tournament participation request is approved
/// Consumed by Tournaments module to register the player
/// </summary>
public class TournamentParticipationApprovedIntegrationEvent : IIntegrationEvent
{
    public Guid TournamentId { get; }
    public string PlayerId { get; }
    public Guid RequestId { get; }

    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public TournamentParticipationApprovedIntegrationEvent(
        Guid tournamentId,
        string playerId,
        Guid requestId
    )
    {
        TournamentId = tournamentId;
        PlayerId = playerId;
        RequestId = requestId;
    }
}
