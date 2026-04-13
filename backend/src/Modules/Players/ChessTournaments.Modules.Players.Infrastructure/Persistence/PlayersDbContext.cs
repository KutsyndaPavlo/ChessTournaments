using ChessTournaments.Modules.Players.Domain.Achievements;
using ChessTournaments.Modules.Players.Domain.Players;
using ChessTournaments.Shared.Domain.Entities;
using ChessTournaments.Shared.Domain.Inbox;
using ChessTournaments.Shared.Domain.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChessTournaments.Modules.Players.Infrastructure.Persistence;

public class PlayersDbContext : DbContext
{
    private readonly IPublisher _publisher;

    public PlayersDbContext(DbContextOptions<PlayersDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Players");

        // Apply configurations from assembly (includes Outbox/Inbox configurations)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlayersDbContext).Assembly);

        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.UserId).IsRequired().HasMaxLength(450);

            entity.HasIndex(p => p.UserId).IsUnique();

            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);

            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);

            entity.Property(p => p.Country).HasMaxLength(100);

            entity.Property(p => p.Bio).HasMaxLength(2000);

            entity.Property(p => p.AvatarUrl).HasMaxLength(500);

            entity.Property(p => p.Rating).IsRequired();

            entity.Property(p => p.PeakRating).IsRequired();

            entity.Property(p => p.PeakRatingDate);

            entity.Property(p => p.TotalGamesPlayed).IsRequired();

            entity.Property(p => p.Wins).IsRequired();

            entity.Property(p => p.Losses).IsRequired();

            entity.Property(p => p.Draws).IsRequired();

            entity.Property(p => p.TournamentsParticipated).IsRequired();

            entity.Property(p => p.TournamentsWon).IsRequired();

            entity.Property(p => p.CreatedAt).IsRequired();

            entity.Property(p => p.UpdatedAt);

            entity.Ignore(p => p.FullName);
            entity.Ignore(p => p.WinRate);
            entity.Ignore(p => p.DrawRate);
            entity.Ignore(p => p.LossRate);
        });

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.ToTable("Achievements");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.PlayerId).IsRequired();
            entity.Property(a => a.TournamentId).IsRequired();
            entity.Property(a => a.TournamentName).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Position).IsRequired();
            entity.Property(a => a.Score).IsRequired().HasColumnType("decimal(5,2)");
            entity.Property(a => a.AchievedAt).IsRequired();
            entity.Property(a => a.CreatedAt).IsRequired();
            entity.Property(a => a.UpdatedAt);

            entity.HasIndex(a => a.PlayerId);
            entity.HasIndex(a => new { a.TournamentId, a.PlayerId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving changes
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
