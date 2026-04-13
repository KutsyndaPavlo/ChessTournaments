using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Modules.Tournaments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Repositories;

public class TournamentRepository : ITournamentRepository
{
    private readonly TournamentsDbContext _context;

    public TournamentRepository(TournamentsDbContext context)
    {
        _context = context;
    }

    public async Task<Tournament?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Tournaments.Include(t => t.Players)
            .Include(t => t.Rounds)
            .ThenInclude(r => r.MatchReferences)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tournament?> GetByIdWithPlayersAndRoundsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Tournaments.Include(t => t.Players)
            .Include(t => t.Rounds)
            .ThenInclude(r => r.MatchReferences)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Tournament>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Tournaments.Include(t => t.Players)
            .Include(t => t.Rounds)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tournament>> GetByOrganizerAsync(
        string organizerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Tournaments.Include(t => t.Players)
            .Include(t => t.Rounds)
            .Where(t => t.OrganizerId == organizerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default)
    {
        await _context.Tournaments.AddAsync(tournament, cancellationToken);
    }

    public async Task UpdateAsync(
        Tournament tournament,
        CancellationToken cancellationToken = default
    )
    {
        _context.Tournaments.Update(tournament);

        if (tournament.Players.Any())
        {
            // Get all existing player IDs for this tournament from the database
            var existingPlayerIds = await _context
                .TournamentPlayers.Where(tp => tp.TournamentId == tournament.Id)
                .Select(tp => tp.Id)
                .ToListAsync(cancellationToken);

            // Filter to only add players that don't already exist in the database
            var newPlayers = tournament
                .Players.Where(p => !existingPlayerIds.Contains(p.Id))
                .ToList();

            if (newPlayers.Any())
                _context.TournamentPlayers.AddRange(newPlayers);
        }

        if (tournament.Rounds.Any())
        {
            // Get all existing round IDs for this tournament from the database
            var existingRoundIds = await _context
                .Rounds.Where(r => r.TournamentId == tournament.Id)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            // Filter to only add rounds that don't already exist in the database
            var newRounds = tournament.Rounds.Where(r => !existingRoundIds.Contains(r.Id)).ToList();

            if (newRounds.Any())
                _context.Rounds.AddRange(newRounds);

            // Note: MatchReferences are owned entities and will be saved automatically with Round
        }
    }

    public Task DeleteAsync(Tournament tournament, CancellationToken cancellationToken = default)
    {
        _context.Tournaments.Remove(tournament);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
