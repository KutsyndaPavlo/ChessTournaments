using ChessTournaments.Modules.Matches.Application.Abstractions;
using ChessTournaments.Modules.Matches.Domain.Matches;
using MediatR;

namespace ChessTournaments.Modules.Matches.Application.Features.GetMatchById;

public class GetMatchByIdQueryHandler : IRequestHandler<GetMatchByIdQuery, TournamentMatchDto?>
{
    private readonly IMatchRepository _matchRepository;

    public GetMatchByIdQueryHandler(IMatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<TournamentMatchDto?> Handle(
        GetMatchByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var match = await _matchRepository.GetByIdAsync(request.MatchId, cancellationToken);

        if (match == null)
            return null;

        return new TournamentMatchDto
        {
            Id = match.Id,
            RoundId = match.RoundId,
            TournamentId = match.TournamentId,
            WhitePlayerId = match.WhitePlayerId,
            BlackPlayerId = match.BlackPlayerId,
            BoardNumber = match.BoardNumber,
            Result = match.Result,
            IsCompleted = match.IsCompleted,
            CompletedAt = match.CompletedAt,
            Moves = match.Moves,
            Tags = match
                .Tags.Select(t => new MatchTagDto
                {
                    Id = t.Id,
                    MatchId = t.MatchId,
                    Name = t.Name,
                })
                .ToList(),
            CreatedAt = match.CreatedAt,
            UpdatedAt = match.UpdatedAt,
        };
    }
}
