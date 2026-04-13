using ChessTournaments.Modules.Players.Domain.Achievements;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Players.Infrastructure.Persistence;

public class AchievementRepository : IAchievementRepository
{
    private readonly PlayersDbContext _context;

    public AchievementRepository(PlayersDbContext context)
    {
        _context = context;
    }

    public async Task<Achievement?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Achievement>> GetByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Achievements.Where(a => a.PlayerId == playerId)
            .OrderByDescending(a => a.AchievedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Achievement?> GetByTournamentAndPlayerAsync(
        Guid tournamentId,
        Guid playerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Achievements.FirstOrDefaultAsync(
            a => a.TournamentId == tournamentId && a.PlayerId == playerId,
            cancellationToken
        );
    }

    public async Task AddAsync(
        Achievement achievement,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Achievements.AddAsync(achievement, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
