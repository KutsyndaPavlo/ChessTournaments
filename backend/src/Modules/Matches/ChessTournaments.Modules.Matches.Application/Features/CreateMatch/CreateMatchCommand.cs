using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.CreateMatch;

public record CreateMatchCommand(
    Guid RoundId,
    Guid TournamentId,
    string WhitePlayerId,
    string BlackPlayerId,
    int BoardNumber
) : IRequest<Result<Guid>>;
