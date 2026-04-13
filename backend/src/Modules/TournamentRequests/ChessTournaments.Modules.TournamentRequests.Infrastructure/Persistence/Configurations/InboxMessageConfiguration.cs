using ChessTournaments.Shared.Domain.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Persistence.Configurations;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired().HasMaxLength(500);

        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.OccurredOnUtc).IsRequired();

        builder.Property(x => x.ProcessedOnUtc);

        builder.Property(x => x.Error).HasMaxLength(2000);

        builder.HasIndex(x => x.ProcessedOnUtc);
        builder.HasIndex(x => x.OccurredOnUtc);
    }
}
