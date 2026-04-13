using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetPendingTournamentRequests;

public record GetPendingTournamentRequestsQuery : IRequest<Result<List<TournamentRequestDto>>>;
