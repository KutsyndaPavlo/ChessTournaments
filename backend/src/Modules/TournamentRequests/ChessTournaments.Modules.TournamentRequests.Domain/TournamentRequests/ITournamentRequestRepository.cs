namespace ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;

public interface ITournamentRequestRepository
{
    Task<TournamentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TournamentRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TournamentRequest>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
    Task<List<TournamentRequest>> GetPendingRequestsAsync(
        CancellationToken cancellationToken = default
    );
    Task AddAsync(TournamentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(TournamentRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
