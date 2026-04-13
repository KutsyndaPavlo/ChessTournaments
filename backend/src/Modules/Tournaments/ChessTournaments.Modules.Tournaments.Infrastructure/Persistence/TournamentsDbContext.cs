using ChessTournaments.Modules.Tournaments.Domain.Rounds;
using ChessTournaments.Modules.Tournaments.Domain.Shared;
using ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Entities;
using ChessTournaments.Shared.Domain.Inbox;
using ChessTournaments.Shared.Domain.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Persistence;

public class TournamentsDbContext : DbContext
{
    public const string SchemaName = "Tournaments";
    private readonly IPublisher _publisher;

    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentPlayer> TournamentPlayers => Set<TournamentPlayer>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public TournamentsDbContext(
        DbContextOptions<TournamentsDbContext> options,
        IPublisher publisher
    )
        : base(options)
    {
        _publisher = publisher;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TournamentsDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
