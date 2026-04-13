using ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Persistence.Configurations;

public class TournamentPlayerConfiguration : IEntityTypeConfiguration<TournamentPlayer>
{
    public void Configure(EntityTypeBuilder<TournamentPlayer> builder)
    {
        builder.ToTable("TournamentPlayers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlayerId).IsRequired().HasMaxLength(450);

        builder.Property(p => p.PlayerName).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Rating);

        builder.Property(p => p.GamesPlayed).IsRequired();

        builder.OwnsOne(
            p => p.TotalScore,
            s =>
            {
                s.Property(x => x.Points)
                    .HasColumnName("TotalScore")
                    .HasColumnType("decimal(18,2)");
            }
        );

        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => new { p.TournamentId, p.PlayerId }).IsUnique();
    }
}
