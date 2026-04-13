using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentById;

public record GetTournamentByIdQuery(Guid TournamentId) : IRequest<Result<TournamentDto>>;
