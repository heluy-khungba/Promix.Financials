using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Aggregates.Accounts;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CompanyId).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200);
        // أضف هذا السطر داخل Configure method:
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.Nature)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.IsPosting).IsRequired();
        builder.Property(x => x.IsSystem).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.CurrencyCode).HasMaxLength(3);
        builder.Property(x => x.SystemRole).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);

        // ✅ Unique account code per company
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();

        // ✅ Unique SystemRole per company (if not null)
        builder.HasIndex(x => new { x.CompanyId, x.SystemRole }).IsUnique()
            .HasFilter("[SystemRole] IS NOT NULL");

        // Self reference (hierarchy)
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}