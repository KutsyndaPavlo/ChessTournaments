using ChessTournaments.Modules.Tournaments.Domain.Shared;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public record TournamentCreatedDomainEvent(Guid TournamentId, string Name, string OrganizerId)
    : DomainEventBase;
