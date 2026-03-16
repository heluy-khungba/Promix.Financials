using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Accounting;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<DefaultCurrency>
{
    public void Configure(EntityTypeBuilder<DefaultCurrency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(10)
            .ValueGeneratedNever();

        builder.Property(x => x.NameAr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Symbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DecimalPlaces)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();
        // أضف هذا السطر داخل Configure method:
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}