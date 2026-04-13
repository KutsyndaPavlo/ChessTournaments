using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.UpdateMatchResult;

public record UpdateMatchResultCommand(
    Guid TournamentId,
    Guid RoundId,
    Guid MatchId,
    string WhitePlayerId,
    string BlackPlayerId,
    int Result
) : IRequest<Result>;
