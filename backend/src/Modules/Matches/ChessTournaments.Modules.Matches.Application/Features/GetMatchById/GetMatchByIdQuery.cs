using ChessTournaments.Modules.Matches.Application.Abstractions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.GetMatchById;

public record GetMatchByIdQuery(Guid MatchId) : IRequest<TournamentMatchDto?>;
