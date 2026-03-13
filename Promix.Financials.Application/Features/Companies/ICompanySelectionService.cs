

namespace Promix.Financials.Application.Features.Companies;

public interface ICompanySelectionService
{
    Task<IReadOnlyList<CompanySummaryDto>> GetMyCompaniesAsync(CancellationToken ct = default);
    Task SelectCompanyAsync(Guid companyId, CancellationToken ct = default);
}
