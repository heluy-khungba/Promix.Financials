using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Accounting;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("CurrencyRates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CompanyId).IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.RateDate)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.CompanyId, x.CurrencyCode, x.RateDate })
            .IsUnique();
    }
}