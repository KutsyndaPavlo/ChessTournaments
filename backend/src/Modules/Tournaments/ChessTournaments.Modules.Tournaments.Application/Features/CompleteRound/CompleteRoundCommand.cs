using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CompleteRound;

public record CompleteRoundCommand(Guid TournamentId, Guid RoundId) : IRequest<Result>;
