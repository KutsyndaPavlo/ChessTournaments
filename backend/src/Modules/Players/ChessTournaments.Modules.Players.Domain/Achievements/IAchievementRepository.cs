namespace ChessTournaments.Modules.Players.Domain.Achievements;

public interface IAchievementRepository
{
    Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Achievement>> GetByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken = default
    );
    Task<Achievement?> GetByTournamentAndPlayerAsync(
        Guid tournamentId,
        Guid playerId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Achievement achievement, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
