using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence.Configurations;

public class TournamentRequestConfiguration : IEntityTypeConfiguration<TournamentRequest>
{
    public void Configure(EntityTypeBuilder<TournamentRequest> builder)
    {
        builder.ToTable("TournamentRequests");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TournamentId).IsRequired();

        builder.Property(t => t.RequestedBy).IsRequired().HasMaxLength(450);

        builder.Property(t => t.ReviewedBy).HasMaxLength(450);

        builder.Property(t => t.RejectionReason).HasMaxLength(1000);

        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(50);

        builder.Property(t => t.ReviewedAt);

        builder.Property(t => t.CreatedAt).IsRequired();

        builder.Property(t => t.UpdatedAt);

        builder.Ignore(t => t.DomainEvents);

        builder.HasIndex(t => t.TournamentId);
        builder.HasIndex(t => t.RequestedBy);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedAt);
    }
}
