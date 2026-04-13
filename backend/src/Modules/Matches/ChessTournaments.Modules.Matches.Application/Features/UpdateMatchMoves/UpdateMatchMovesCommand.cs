using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.UpdateMatchMoves;

public record UpdateMatchMovesCommand(Guid MatchId, string Moves) : IRequest<Result>;
