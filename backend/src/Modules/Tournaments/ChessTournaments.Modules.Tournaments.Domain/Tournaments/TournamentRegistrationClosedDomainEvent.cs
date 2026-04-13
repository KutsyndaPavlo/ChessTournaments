using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentRegistrationClosedDomainEvent(Guid TournamentId, int PlayerCount)
    : DomainEventBase;
