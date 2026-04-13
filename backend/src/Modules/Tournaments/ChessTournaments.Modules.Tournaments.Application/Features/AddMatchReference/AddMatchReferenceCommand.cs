using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.AddMatchReference;

public record AddMatchReferenceCommand(
    Guid TournamentId,
    Guid RoundId,
    Guid MatchId,
    int BoardNumber
) : IRequest<Result>;
