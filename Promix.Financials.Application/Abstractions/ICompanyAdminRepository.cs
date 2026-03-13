using Promix.Financials.Domain.Security;

namespace Promix.Financials.Application.Abstractions;

public interface ICompanyAdminRepository
{
    Task<bool> CompanyCodeExistsAsync(string code, CancellationToken ct = default);
    Task<string> GenerateNextCompanyCodeAsync(CancellationToken ct = default);
    Task<Company> CreateCompanyAsync(
        string code,
        string name,
        string baseCurrency,
        Guid ownerUserId,
        CancellationToken ct = default);
}