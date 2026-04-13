using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Rounds;

public record RoundCompletedDomainEvent(Guid TournamentId, Guid RoundId, int RoundNumber)
    : DomainEventBase;
