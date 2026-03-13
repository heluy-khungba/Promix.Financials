using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Promix.Financials.Infrastructure.Persistence;

public sealed class PromixDbContextFactory : IDesignTimeDbContextFactory<PromixDbContext>
{
    public PromixDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PromixDbContext>();

        // Dev default (غيّره لاحقًا لما نربطه من UI settings)
        var cs =
    "Data Source=.\\MSSQLSERVER2025;" +
    "Initial Catalog=PromixFinancials;" +
    "Integrated Security=True;" +
    "Encrypt=True;" +
    "TrustServerCertificate=True;" +
    "MultipleActiveResultSets=True;";

        options.UseSqlServer(cs);
        return new PromixDbContext(options.Options);
    }
}