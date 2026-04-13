using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.RejectTournamentRequest;

public record RejectTournamentRequestCommand(Guid RequestId, string AdminId, string RejectionReason)
    : IRequest<Result<TournamentRequestDto>>;
