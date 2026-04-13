using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetAllTournamentRequests;

public record GetAllTournamentRequestsQuery : IRequest<Result<List<TournamentRequestDto>>>;
