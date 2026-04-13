using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CreateRoundWithPairings;

public record CreateRoundWithPairingsCommand(Guid TournamentId) : IRequest<Result<RoundDto>>;
