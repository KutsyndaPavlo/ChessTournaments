using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetAllTournamentRequests;

public class GetAllTournamentRequestsQueryHandler
    : IRequestHandler<GetAllTournamentRequestsQuery, Result<List<TournamentRequestDto>>>
{
    private readonly ITournamentRequestRepository _repository;

    public GetAllTournamentRequestsQueryHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TournamentRequestDto>>> Handle(
        GetAllTournamentRequestsQuery request,
        CancellationToken cancellationToken
    )
    {
        var requests = await _repository.GetAllAsync(cancellationToken);

        var dtos = requests
            .Select(r => new TournamentRequestDto
            {
                Id = r.Id,
                TournamentId = r.TournamentId,
                RequestedBy = r.RequestedBy,
                Status = r.Status,
                ReviewedBy = r.ReviewedBy,
                ReviewedAt = r.ReviewedAt,
                RejectionReason = r.RejectionReason,
                CreatedAt = r.CreatedAt,
            })
            .ToList();

        return Result.Success(dtos);
    }
}
