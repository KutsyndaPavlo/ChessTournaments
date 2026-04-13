using ChessTournaments.Modules.Players.Domain.Players;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Players.Infrastructure.Persistence;

public class PlayerRepository : IPlayerRepository
{
    private readonly PlayersDbContext _context;

    public PlayerRepository(PlayersDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Players.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Player?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Players.FirstOrDefaultAsync(
            p => p.UserId == userId,
            cancellationToken
        );
    }

    public async Task<List<Player>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context
            .Players.OrderByDescending(p => p.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Player>> GetTopByRatingAsync(
        int count,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Players.OrderByDescending(p => p.Rating)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Player>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context
            .Players.Where(p =>
                p.FirstName.ToLower().Contains(lowerSearchTerm)
                || p.LastName.ToLower().Contains(lowerSearchTerm)
            )
            .OrderByDescending(p => p.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Player player, CancellationToken cancellationToken = default)
    {
        await _context.Players.AddAsync(player, cancellationToken);
    }

    public Task UpdateAsync(Player player, CancellationToken cancellationToken = default)
    {
        _context.Players.Update(player);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
