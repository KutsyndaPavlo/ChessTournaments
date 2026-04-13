using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentRegistrationOpenedDomainEvent(Guid TournamentId) : DomainEventBase;
