using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Infrastructure.Persistence;

namespace Promix.Financials.Infrastructure.Persistence.Repositories;

public sealed class EfAccountRepository : IAccountRepository
{
    private readonly PromixDbContext _db;

    public EfAccountRepository(PromixDbContext db) => _db = db;

    // ─── موجودة ✅ ────────────────────────────────────────────────
    public Task<bool> CodeExistsAsync(Guid companyId, string code)
        => _db.Accounts.AnyAsync(a => a.CompanyId == companyId && a.Code == code);

    public Task<bool> SystemRoleExistsAsync(Guid companyId, string systemRole)
        => _db.Accounts.AnyAsync(a => a.CompanyId == companyId && a.SystemRole == systemRole);

    public Task<Account?> GetByIdAsync(Guid id)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);

    public Task AddAsync(Account account)
    {
        _db.Accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();

    // ─── جديدة 🆕 ────────────────────────────────────────────────
    public Task<Account?> GetByIdAsync(Guid id, Guid companyId)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.CompanyId == companyId);

    public async Task<IReadOnlyList<Account>> GetPostingAccountsAsync(Guid companyId)
        => await _db.Accounts
            .AsNoTracking()
            .Where(a => a.CompanyId == companyId && a.IsPosting && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync();

    public Task<bool> HasChildrenAsync(Guid accountId, Guid companyId)
        => _db.Accounts.AnyAsync(a => a.ParentId == accountId && a.CompanyId == companyId);

    public Task<bool> HasMovementsAsync(Guid accountId, Guid companyId)
        => _db.JournalLines
            .AnyAsync(x => x.AccountId == accountId && x.JournalEntry.CompanyId == companyId);

    public void Remove(Account account)
        => _db.Accounts.Remove(account);
}
