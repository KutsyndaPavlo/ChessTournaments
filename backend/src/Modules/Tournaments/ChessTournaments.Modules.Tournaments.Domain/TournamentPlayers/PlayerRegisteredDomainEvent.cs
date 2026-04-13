using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;

public record PlayerRegisteredDomainEvent(Guid TournamentId, string PlayerId, string PlayerName)
    : DomainEventBase;
