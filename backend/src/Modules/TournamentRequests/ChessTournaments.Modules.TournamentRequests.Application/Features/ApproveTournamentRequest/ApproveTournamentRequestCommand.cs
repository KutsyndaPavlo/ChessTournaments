using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.ApproveTournamentRequest;

public record ApproveTournamentRequestCommand(Guid RequestId, string AdminId)
    : IRequest<Result<TournamentRequestDto>>;
