using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentRounds;

public class GetTournamentRoundsQueryHandler
    : IRequestHandler<GetTournamentRoundsQuery, IEnumerable<RoundDto>>
{
    private readonly ITournamentRepository _repository;

    public GetTournamentRoundsQueryHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RoundDto>> Handle(
        GetTournamentRoundsQuery request,
        CancellationToken cancellationToken
    )
    {
        var tournament =
            await _repository.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new InvalidOperationException("Tournament not found");

        return tournament
            .Rounds.OrderBy(r => r.RoundNumber)
            .Select(r => new RoundDto
            {
                Id = r.Id,
                TournamentId = r.TournamentId,
                RoundNumber = r.RoundNumber,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                IsCompleted = r.IsCompleted,
                MatchCount = r.MatchCount,
                Matches = new List<MatchDto>(), // Match details should be fetched from Matches module
            });
    }
}
