using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;

public record PlayerUnregisteredDomainEvent(Guid TournamentId, string PlayerId) : DomainEventBase;
