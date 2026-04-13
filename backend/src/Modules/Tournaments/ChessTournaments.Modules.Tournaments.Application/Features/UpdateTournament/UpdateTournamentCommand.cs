using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;

public record UpdateTournamentCommand(
    Guid TournamentId,
    string Name,
    string Description,
    string Location
) : IRequest<Result<TournamentDto>>;
