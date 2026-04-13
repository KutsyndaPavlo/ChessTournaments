using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Persistence.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);

        builder.Property(t => t.Description).HasMaxLength(2000);

        builder.Property(t => t.Location).HasMaxLength(200);

        builder.Property(t => t.OrganizerId).IsRequired().HasMaxLength(450);

        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);

        builder.Property(t => t.StartDate).IsRequired();

        builder.Property(t => t.EndDate);

        builder.OwnsOne(
            t => t.Settings,
            s =>
            {
                s.Property(x => x.Format).HasConversion<string>().HasMaxLength(50);
                s.Property(x => x.TimeControl).HasConversion<string>().HasMaxLength(50);
                s.Property(x => x.TimeInMinutes).IsRequired();
                s.Property(x => x.IncrementInSeconds).IsRequired();
                s.Property(x => x.NumberOfRounds).IsRequired();
                s.Property(x => x.MaxPlayers).IsRequired();
                s.Property(x => x.MinPlayers).IsRequired();
                s.Property(x => x.AllowByes).IsRequired();
                s.Property(x => x.EntryFee).HasColumnType("decimal(18,2)");
            }
        );

        builder
            .HasMany(t => t.Players)
            .WithOne()
            .HasForeignKey(p => p.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(t => t.Players)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_players");

        builder
            .HasMany(t => t.Rounds)
            .WithOne()
            .HasForeignKey(r => r.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(t => t.Rounds)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_rounds");

        builder.Ignore(t => t.DomainEvents);

        builder.HasIndex(t => t.OrganizerId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.StartDate);
    }
}
