using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentCancelledDomainEvent(Guid TournamentId) : DomainEventBase;
