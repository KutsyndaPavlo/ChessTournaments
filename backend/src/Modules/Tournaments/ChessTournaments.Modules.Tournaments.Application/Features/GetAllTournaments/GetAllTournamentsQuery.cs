using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetAllTournaments;

public record GetAllTournamentsQuery : IRequest<IEnumerable<TournamentDto>>;
