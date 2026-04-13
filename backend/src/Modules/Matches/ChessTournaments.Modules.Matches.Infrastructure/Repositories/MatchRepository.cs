using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Modules.Matches.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Matches.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{
    private readonly MatchesDbContext _context;

    public MatchRepository(MatchesDbContext context)
    {
        _context = context;
    }

    public async Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Matches.Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Match>> GetByRoundIdAsync(
        Guid roundId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Matches.Include(m => m.Tags)
            .Where(m => m.RoundId == roundId)
            .OrderBy(m => m.BoardNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Match>> GetByTournamentIdAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Matches.Include(m => m.Tags)
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Match>> SearchByPlayerIdAsync(
        string playerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Matches.Include(m => m.Tags)
            .Where(m => m.WhitePlayerId == playerId || m.BlackPlayerId == playerId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Match>> SearchByTagsAsync(
        string[] tags,
        CancellationToken cancellationToken = default
    )
    {
        if (tags == null || tags.Length == 0)
            return Enumerable.Empty<Match>();

        var query = _context.Matches.Include(m => m.Tags).AsQueryable();

        foreach (var tag in tags)
        {
            query = query.Where(m => m.Tags.Any(t => t.Name.ToLower() == tag.ToLower()));
        }

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Match>> SearchByPlayerAndTagsAsync(
        string playerId,
        string[] tags,
        CancellationToken cancellationToken = default
    )
    {
        var query = _context
            .Matches.Include(m => m.Tags)
            .Where(m => m.WhitePlayerId == playerId || m.BlackPlayerId == playerId);

        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                query = query.Where(m => m.Tags.Any(t => t.Name.ToLower() == tag.ToLower()));
            }
        }

        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Match match, CancellationToken cancellationToken = default)
    {
        await _context.Matches.AddAsync(match, cancellationToken);
    }

    public Task UpdateAsync(Match match, CancellationToken cancellationToken = default)
    {
        _context.Matches.Update(match);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Match match, CancellationToken cancellationToken = default)
    {
        _context.Matches.Remove(match);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
