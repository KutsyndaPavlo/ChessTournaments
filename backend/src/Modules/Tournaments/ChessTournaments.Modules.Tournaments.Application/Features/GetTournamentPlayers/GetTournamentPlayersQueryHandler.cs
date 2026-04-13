using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentPlayers;

public class GetTournamentPlayersQueryHandler
    : IRequestHandler<GetTournamentPlayersQuery, IEnumerable<TournamentPlayerDto>>
{
    private readonly ITournamentRepository _repository;

    public GetTournamentPlayersQueryHandler(ITournamentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TournamentPlayerDto>> Handle(
        GetTournamentPlayersQuery request,
        CancellationToken cancellationToken
    )
    {
        var tournament =
            await _repository.GetByIdAsync(request.TournamentId, cancellationToken)
            ?? throw new InvalidOperationException("Tournament not found");

        return tournament
            .Players.OrderByDescending(p => p.TotalScore.Points)
            .ThenByDescending(p => p.Rating ?? 0)
            .Select(p => new TournamentPlayerDto
            {
                Id = p.Id,
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName,
                Rating = p.Rating,
                TotalScore = p.TotalScore.Points,
                GamesPlayed = p.GamesPlayed,
            });
    }
}
