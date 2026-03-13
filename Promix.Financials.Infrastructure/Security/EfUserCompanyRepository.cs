using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Companies;
using Promix.Financials.Infrastructure.Persistence;

namespace Promix.Financials.Infrastructure.Security;

public sealed class EfUserCompanyRepository : IUserCompanyRepository
{
    private readonly PromixDbContext _db;

    public EfUserCompanyRepository(PromixDbContext db)
    {
        _db = db;
    }

    // ✅ FIX 1: إزالة AsNoTracking لأننا نرجع Guid وليس Entity
    public async Task<IReadOnlyList<Guid>> GetCompanyIdsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserCompanies
            .Where(x => x.UserId == userId)
            .Select(x => x.CompanyId)
            .ToListAsync(ct);
    }

    // ✅ FIX 2: استخدام الخصائص الصحيحة من Company (Name فقط)
    public async Task<IReadOnlyList<CompanySummaryDto>> GetCompaniesForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserCompanies
            .Where(uc => uc.UserId == userId)
            .Join(
                _db.Companies.Where(c => c.IsActive), // فقط الشركات الفعالة
                uc => uc.CompanyId,
                c => c.Id,
                (uc, c) => new CompanySummaryDto(
                    c.Id,
                    c.Name,      // ArabicName تمثل Name
                    null,        // لا يوجد EnglishName حالياً
                    c.BaseCurrency
                )
            )
            .AsNoTracking()
            .ToListAsync(ct);
    }
}