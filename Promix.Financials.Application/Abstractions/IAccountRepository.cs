using Promix.Financials.Domain.Aggregates.Accounts;

namespace Promix.Financials.Application.Abstractions;

public interface IAccountRepository
{
    // ─── موجودة ✅ ────────────────────────────────────────────────
    Task<bool> CodeExistsAsync(Guid companyId, string code);
    Task<bool> SystemRoleExistsAsync(Guid companyId, string systemRole);
    Task<Account?> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
    Task SaveChangesAsync();

    // ─── جديدة 🆕 ────────────────────────────��───────────────────
    Task<Account?> GetByIdAsync(Guid id, Guid companyId);
    Task<IReadOnlyList<Account>> GetPostingAccountsAsync(Guid companyId);
    Task<bool> HasChildrenAsync(Guid accountId, Guid companyId);
    Task<bool> HasMovementsAsync(Guid accountId, Guid companyId);
    void Remove(Account account);
}
