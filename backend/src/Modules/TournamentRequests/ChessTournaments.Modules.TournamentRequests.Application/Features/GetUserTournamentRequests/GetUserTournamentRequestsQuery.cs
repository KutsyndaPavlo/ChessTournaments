using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetUserTournamentRequests;

public record GetUserTournamentRequestsQuery(string UserId)
    : IRequest<Result<List<TournamentRequestDto>>>;
