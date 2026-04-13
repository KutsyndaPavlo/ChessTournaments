using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.CancelTournamentRequest;

public record CancelTournamentRequestCommand(Guid RequestId)
    : IRequest<Result<TournamentRequestDto>>;
