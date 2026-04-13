using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.GetUserTournamentRequests;

public class GetUserTournamentRequestsQueryHandler
    : IRequestHandler<GetUserTournamentRequestsQuery, Result<List<TournamentRequestDto>>>
{
    private readonly ITournamentRequestRepository _repository;

    public GetUserTournamentRequestsQueryHandler(ITournamentRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TournamentRequestDto>>> Handle(
        GetUserTournamentRequestsQuery request,
        CancellationToken cancellationToken
    )
    {
        var requests = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

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
