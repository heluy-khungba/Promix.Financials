using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Security;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable("UserCompanies");

        // ✅ Composite PK
        builder.HasKey(x => new { x.UserId, x.CompanyId });

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CompanyId).IsRequired();

        // (اختياري لكن مفيد للأداء)
        builder.HasIndex(x => x.CompanyId);
    }
}