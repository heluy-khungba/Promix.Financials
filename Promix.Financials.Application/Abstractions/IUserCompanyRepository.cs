using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Promix.Financials.Application.Features.Companies;

namespace Promix.Financials.Application.Abstractions;

public interface IUserCompanyRepository
{
    // موجودة عندك (لا نحذفها)
    Task<IReadOnlyList<Guid>> GetCompanyIdsForUserAsync(Guid userId, CancellationToken ct = default);

    // ✅ NEW
    Task<IReadOnlyList<CompanySummaryDto>> GetCompaniesForUserAsync(Guid userId, CancellationToken ct = default);
}