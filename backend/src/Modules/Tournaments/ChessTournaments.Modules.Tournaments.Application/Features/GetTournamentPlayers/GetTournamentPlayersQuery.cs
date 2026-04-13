using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentPlayers;

public record GetTournamentPlayersQuery(Guid TournamentId)
    : IRequest<IEnumerable<TournamentPlayerDto>>;
