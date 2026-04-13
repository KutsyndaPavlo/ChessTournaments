namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public interface ITournamentRepository
{
    Task<Tournament?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tournament?> GetByIdWithPlayersAndRoundsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<Tournament>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tournament>> GetByOrganizerAsync(
        string organizerId,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tournament tournament, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
