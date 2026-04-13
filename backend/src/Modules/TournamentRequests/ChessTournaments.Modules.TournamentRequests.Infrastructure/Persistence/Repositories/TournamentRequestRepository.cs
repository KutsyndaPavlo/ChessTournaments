using ChessTournaments.Modules.TournamentRequests.Domain.Enums;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence.Repositories;

public class TournamentRequestRepository : ITournamentRequestRepository
{
    private readonly TournamentRequestsDbContext _context;
    private readonly ILogger<TournamentRequestRepository>? _logger;

    public TournamentRequestRepository(
        TournamentRequestsDbContext context,
        ILogger<TournamentRequestRepository>? logger = null
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TournamentRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.TournamentRequests.FirstOrDefaultAsync(
            t => t.Id == id,
            cancellationToken
        );
    }

    public async Task<List<TournamentRequest>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .TournamentRequests.OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TournamentRequest>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .TournamentRequests.Where(t => t.RequestedBy == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TournamentRequest>> GetPendingRequestsAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .TournamentRequests.Where(t => t.Status == RequestStatus.Pending)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TournamentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await _context.TournamentRequests.AddAsync(request, cancellationToken);
    }

    public Task UpdateAsync(
        TournamentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _context.TournamentRequests.Update(request);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pendingChanges = _context
            .ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .ToList();

        _logger?.LogInformation(
            "SaveChangesAsync called. DbContext: {DbContextType}, Instance: {DbContextHashCode}, Pending changes: {ChangeCount}",
            _context.GetType().Name,
            _context.GetHashCode(),
            pendingChanges.Count
        );

        foreach (var entry in pendingChanges)
        {
            _logger?.LogInformation(
                "  - {EntityType} ({EntityId}): {State}",
                entry.Entity.GetType().Name,
                entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue,
                entry.State
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation("SaveChangesAsync completed successfully");
    }
}
