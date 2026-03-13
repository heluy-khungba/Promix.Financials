using Promix.Financials.Domain.Aggregates.Accounts;

namespace Promix.Financials.Application.Abstractions;

public interface IAccountRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code);
    Task<bool> SystemRoleExistsAsync(Guid companyId, string systemRole);

    Task<Account?> GetByIdAsync(Guid id);

    Task AddAsync(Account account);
    Task SaveChangesAsync();
}
