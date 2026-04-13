using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetPendingTournamentRequests;

public class GetPendingTournamentRequestsQueryHandler
    : IRequestHandler<GetPendingTournamentRequestsQuery, Result<List<TournamentRequestDto>>>
{
    private readonly ITournamentRequestRepository _repository;

    public GetPendingTournamentRequestsQueryHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TournamentRequestDto>>> Handle(
        GetPendingTournamentRequestsQuery request,
        CancellationToken cancellationToken
    )
    {
        var requests = await _repository.GetPendingRequestsAsync(cancellationToken);

        var dtos = requests
            .Select(r => new TournamentRequestDto
            {
                Id = r.Id,
                TournamentId = r.TournamentId,
                RequestedBy = r.RequestedBy,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
            })
            .ToList();

        return Result.Success(dtos);
    }
}
