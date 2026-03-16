using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Accounting;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class CompanyCurrencyConfiguration : IEntityTypeConfiguration<CompanyCurrency>
{
    public void Configure(EntityTypeBuilder<CompanyCurrency> builder)
    {
        builder.ToTable("CompanyCurrencies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        // أضف هذا السطر داخل Configure method:
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.CompanyId).IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.NameAr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NameEn)
            .HasMaxLength(100);

        builder.Property(x => x.Symbol)
            .HasMaxLength(20);

        builder.Property(x => x.DecimalPlaces).IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(18, 8)
            .IsRequired();

        builder.Property(x => x.IsBaseCurrency).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        // ← ع��لة واحدة رئيسية per Company
        builder.HasIndex(x => new { x.CompanyId, x.IsBaseCurrency })
            .IsUnique()
            .HasFilter("[IsBaseCurrency] = 1");

        // ← لا تكرار للكود داخل نفس الشركة
        builder.HasIndex(x => new { x.CompanyId, x.CurrencyCode })
            .IsUnique();
    }
}