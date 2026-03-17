using Microsoft.EntityFrameworkCore;
using Promix.Financials.Domain.Accounting;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Domain.Aggregates.Journals;
using Promix.Financials.Domain.Security;

namespace Promix.Financials.Infrastructure.Persistence;

public sealed class PromixDbContext : DbContext
{
    public PromixDbContext(DbContextOptions<PromixDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<CompanyCurrency> CompanyCurrencies => Set<CompanyCurrency>();
    public DbSet<DefaultCurrency> Currencies => Set<DefaultCurrency>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromixDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
