using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Shared.Domain.Inbox;
using ChessTournaments.Shared.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Matches.Infrastructure.Persistence;

public class MatchesDbContext : DbContext
{
    public const string SchemaName = "Matches";

    public MatchesDbContext(DbContextOptions<MatchesDbContext> options)
        : base(options) { }

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchTag> MatchTags => Set<MatchTag>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MatchesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
