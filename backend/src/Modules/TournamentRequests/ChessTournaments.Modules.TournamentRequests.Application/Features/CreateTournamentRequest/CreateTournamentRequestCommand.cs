using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.CreateTournamentRequest;

public record CreateTournamentRequestCommand(Guid TournamentId, string RequestedBy)
    : IRequest<Result<TournamentRequestDto>>;
