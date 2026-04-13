using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentRounds;

public record GetTournamentRoundsQuery(Guid TournamentId) : IRequest<IEnumerable<RoundDto>>;
