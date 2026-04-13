namespace ChessTournaments.Modules.Players.Domain.Players;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Player?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Player>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Player>> GetTopByRatingAsync(
        int count,
        CancellationToken cancellationToken = default
    );
    Task<List<Player>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Player player, CancellationToken cancellationToken = default);
    Task UpdateAsync(Player player, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
