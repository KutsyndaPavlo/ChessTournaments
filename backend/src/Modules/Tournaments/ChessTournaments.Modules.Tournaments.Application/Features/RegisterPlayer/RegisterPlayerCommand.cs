using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;

public record RegisterPlayerCommand(
    Guid TournamentId,
    string PlayerId,
    string PlayerName,
    int? Rating
) : IRequest<Result>;
