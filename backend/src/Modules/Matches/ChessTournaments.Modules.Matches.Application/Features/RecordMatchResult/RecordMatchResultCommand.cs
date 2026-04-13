using ChessTournaments.Shared.Domain.Enums;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.RecordMatchResult;

public record RecordMatchResultCommand(Guid MatchId, GameResult Result, string? Moves = null)
    : IRequest<Result>;
