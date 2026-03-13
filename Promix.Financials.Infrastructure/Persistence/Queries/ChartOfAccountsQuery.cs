using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Infrastructure.Persistence;

namespace Promix.Financials.Infrastructure.Persistence.Queries;

internal sealed class ChartOfAccountsQuery : IChartOfAccountsQuery
{
    private readonly PromixDbContext _db;

    public ChartOfAccountsQuery(PromixDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AccountFlatDto>> GetAccountsAsync(Guid companyId)
    {
        return await _db.Set<Account>()
            .AsNoTracking()
            .Where(a => a.CompanyId == companyId)
            .OrderBy(a => a.Code)
            .Select(a => new AccountFlatDto(
                a.Id,
                a.ParentId,
                a.Code,
                a.NameAr,      // ✅ بدل ArabicName
                a.IsPosting,   // ✅ بدل Type
                a.IsSystem,    // ✅ بدل SystemRole
                a.IsActive
            ))
            .ToListAsync();
    }
}