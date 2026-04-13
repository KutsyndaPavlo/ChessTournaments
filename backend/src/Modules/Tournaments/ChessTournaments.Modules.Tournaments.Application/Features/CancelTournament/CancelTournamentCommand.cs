using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CancelTournament;

public record CancelTournamentCommand(Guid TournamentId) : IRequest<Result>;
