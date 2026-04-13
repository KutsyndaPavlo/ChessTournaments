using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CompleteTournament;

public record CompleteTournamentCommand(Guid TournamentId) : IRequest<Result>;
