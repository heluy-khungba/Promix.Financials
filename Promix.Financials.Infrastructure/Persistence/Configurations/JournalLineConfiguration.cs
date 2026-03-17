using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Aggregates.Journals;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("JournalLines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Property(x => x.JournalEntryId).IsRequired();
        builder.Property(x => x.LineNumber).IsRequired();
        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.Debit).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Credit).HasPrecision(18, 2).IsRequired();

        builder.HasIndex(x => new { x.JournalEntryId, x.LineNumber }).IsUnique();

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
