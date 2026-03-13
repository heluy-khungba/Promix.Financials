using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Promix.Financials.Application.Features.Accounts;

public sealed record AccountFlatDto(
    Guid Id,
    Guid? ParentId,
    string Code,
    string ArabicName,
    bool IsPosting,
    bool IsSystem,
    bool IsActive);

public interface IChartOfAccountsQuery
{
    Task<IReadOnlyList<AccountFlatDto>> GetAccountsAsync(Guid companyId);
}