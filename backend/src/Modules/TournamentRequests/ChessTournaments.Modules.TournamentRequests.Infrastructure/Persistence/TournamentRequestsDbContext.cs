using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using ChessTournaments.Shared.Domain.Inbox;
using ChessTournaments.Shared.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence;

public class TournamentRequestsDbContext : DbContext
{
    public const string SchemaName = "TournamentRequests";

    public DbSet<TournamentRequest> TournamentRequests => Set<TournamentRequest>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public TournamentRequestsDbContext(DbContextOptions<TournamentRequestsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TournamentRequestsDbContext).Assembly);
    }
}
