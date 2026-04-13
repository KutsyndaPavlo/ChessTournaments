using ChessTournaments.Modules.Matches.Application.Abstractions;
using ChessTournaments.Modules.Matches.Domain.Matches;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.SearchMatches;

public class SearchMatchesQueryHandler
    : IRequestHandler<SearchMatchesQuery, IEnumerable<TournamentMatchDto>>
{
    private readonly IMatchRepository _matchRepository;

    public SearchMatchesQueryHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<IEnumerable<TournamentMatchDto>> Handle(
        SearchMatchesQuery request,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<Match> matches;

        // Prioritize specific queries
        if (request.RoundId.HasValue)
        {
            matches = await _matchRepository.GetByRoundIdAsync(
                request.RoundId.Value,
                cancellationToken
            );
        }
        else if (request.TournamentId.HasValue)
        {
            matches = await _matchRepository.GetByTournamentIdAsync(
                request.TournamentId.Value,
                cancellationToken
            );
        }
        else if (!string.IsNullOrWhiteSpace(request.PlayerId) && request.Tags?.Length > 0)
        {
            matches = await _matchRepository.SearchByPlayerAndTagsAsync(
                request.PlayerId,
                request.Tags,
                cancellationToken
            );
        }
        else if (!string.IsNullOrWhiteSpace(request.PlayerId))
        {
            matches = await _matchRepository.SearchByPlayerIdAsync(
                request.PlayerId,
                cancellationToken
            );
        }
        else if (request.Tags?.Length > 0)
        {
            matches = await _matchRepository.SearchByTagsAsync(request.Tags, cancellationToken);
        }
        else
        {
            return Enumerable.Empty<TournamentMatchDto>();
        }

        return matches.Select(m => new TournamentMatchDto
        {
            Id = m.Id,
            RoundId = m.RoundId,
            TournamentId = m.TournamentId,
            WhitePlayerId = m.WhitePlayerId,
            BlackPlayerId = m.BlackPlayerId,
            BoardNumber = m.BoardNumber,
            Result = m.Result,
            IsCompleted = m.IsCompleted,
            CompletedAt = m.CompletedAt,
            ScheduledTime = m.CreatedAt, // Using CreatedAt as scheduled time
            Moves = m.Moves,
            Tags = m
                .Tags.Select(t => new MatchTagDto
                {
                    Id = t.Id,
                    MatchId = t.MatchId,
                    Name = t.Name,
                })
                .ToList(),
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
        });
    }
}
