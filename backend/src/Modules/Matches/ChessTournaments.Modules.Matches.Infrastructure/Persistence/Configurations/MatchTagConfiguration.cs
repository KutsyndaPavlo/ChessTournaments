using ChessTournaments.Modules.Matches.Domain.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.Matches.Infrastructure.Persistence.Configurations;

public class MatchTagConfiguration : IEntityTypeConfiguration<MatchTag>
{
    public void Configure(EntityTypeBuilder<MatchTag> builder)
    {
        builder.ToTable("MatchTags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.MatchId).IsRequired();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt);

        builder.HasIndex(t => t.MatchId);
        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => new { t.MatchId, t.Name }).IsUnique();

        builder.Ignore(t => t.DomainEvents);
    }
}
