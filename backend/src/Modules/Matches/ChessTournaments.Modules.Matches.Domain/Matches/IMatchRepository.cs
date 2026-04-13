namespace ChessTournaments.Modules.Matches.Domain.Matches;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Match>> GetByRoundIdAsync(
        Guid roundId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Match>> GetByTournamentIdAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Match>> SearchByPlayerIdAsync(
        string playerId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Match>> SearchByTagsAsync(
        string[] tags,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Match>> SearchByPlayerAndTagsAsync(
        string playerId,
        string[] tags,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Match match, CancellationToken cancellationToken = default);
    Task UpdateAsync(Match match, CancellationToken cancellationToken = default);
    Task DeleteAsync(Match match, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
