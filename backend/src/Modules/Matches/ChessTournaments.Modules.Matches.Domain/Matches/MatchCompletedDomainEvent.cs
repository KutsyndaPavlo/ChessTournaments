using ChessTournaments.Shared.Domain.Enums;
using ChessTournaments.Shared.Domain.Events;

namespace ChessTournaments.Modules.Matches.Domain.Matches;

public record MatchCompletedDomainEvent(
    Guid TournamentId,
    Guid RoundId,
    Guid MatchId,
    string WhitePlayerId,
    string BlackPlayerId,
    GameResult Result
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
