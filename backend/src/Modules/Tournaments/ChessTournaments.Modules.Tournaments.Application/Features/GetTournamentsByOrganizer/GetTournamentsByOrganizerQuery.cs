using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentsByOrganizer;

public record GetTournamentsByOrganizerQuery(string OrganizerId)
    : IRequest<IEnumerable<TournamentDto>>;
