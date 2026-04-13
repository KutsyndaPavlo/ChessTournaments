using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentCompletedDomainEvent(Guid TournamentId) : DomainEventBase;
