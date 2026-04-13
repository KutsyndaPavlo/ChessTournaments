using ChessTournaments.Modules.Matches.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Matches.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.RoundId).IsRequired();
        builder.Property(m => m.TournamentId).IsRequired();
        builder.Property(m => m.WhitePlayerId).IsRequired().HasMaxLength(256);
        builder.Property(m => m.BlackPlayerId).IsRequired().HasMaxLength(256);
        builder.Property(m => m.BoardNumber).IsRequired();
        builder.Property(m => m.Result).IsRequired().HasConversion<string>();
        builder.Property(m => m.IsCompleted).IsRequired();
        builder.Property(m => m.CompletedAt);
        builder.Property(m => m.Moves).HasMaxLength(10000);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt);

        builder
            .HasMany(m => m.Tags)
            .WithOne()
            .HasForeignKey(t => t.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.RoundId);
        builder.HasIndex(m => m.TournamentId);
        builder.HasIndex(m => m.WhitePlayerId);
        builder.HasIndex(m => m.BlackPlayerId);
        builder.HasIndex(m => m.IsCompleted);

        builder.Ignore(m => m.DomainEvents);
    }
}
