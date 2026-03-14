using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;   // ✅ هنا وليس Features.Accounts.Abstractions
using Promix.Financials.Domain.Aggregates.Accounts;

namespace Promix.Financials.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly PromixDbContext _db;

    public AccountRepository(PromixDbContext db)
        => _db = db;

    // ─── موجودة في EfAccountRepository ✅ ─────────────────────────
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

    // ─── جديدة 🆕 ─────────────────────────────────────────────────
    public Task<Account?> GetByIdAsync(Guid id, Guid companyId)
        => _db.Accounts.FirstOrDefaultAsync(a =>
               a.Id == id && a.CompanyId == companyId);

    public Task<bool> HasChildrenAsync(Guid accountId, Guid companyId)
        => _db.Accounts.AnyAsync(a =>
               a.ParentId == accountId && a.CompanyId == companyId);

    public Task<bool> HasMovementsAsync(Guid accountId, Guid companyId)
        // ✅ مؤقتاً false — يُحدَّث عند بناء JournalLines
        => Task.FromResult(false);

    public Task SaveChangesAsync()
        => _db.SaveChangesAsync();

    public void Remove(Account account)
        => _db.Accounts.Remove(account);
}