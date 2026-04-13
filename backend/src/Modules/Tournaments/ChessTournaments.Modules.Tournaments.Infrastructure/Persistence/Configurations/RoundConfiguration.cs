using ChessTournaments.Modules.Tournaments.Domain.Rounds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoundNumber).IsRequired();

        builder.Property(r => r.IsCompleted).IsRequired();

        builder.Property(r => r.StartTime);

        builder.Property(r => r.EndTime);

        // Configure MatchReferences as owned entities
        builder.OwnsMany(
            r => r.MatchReferences,
            mb =>
            {
                mb.ToTable("RoundMatches");
                mb.WithOwner().HasForeignKey("RoundId");
                mb.Property<Guid>("Id");
                mb.HasKey("Id");
                mb.Property(m => m.MatchId).IsRequired();
                mb.Property(m => m.BoardNumber).IsRequired();
                mb.Property(m => m.IsCompleted).IsRequired();
                mb.HasIndex(m => m.MatchId);
            }
        );

        builder.Ignore(r => r.DomainEvents);

        builder.HasIndex(r => new { r.TournamentId, r.RoundNumber }).IsUnique();
    }
}
