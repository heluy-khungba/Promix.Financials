using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Promix.Financials.Domain.Security;

namespace Promix.Financials.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .IsRequired();
    }
}
