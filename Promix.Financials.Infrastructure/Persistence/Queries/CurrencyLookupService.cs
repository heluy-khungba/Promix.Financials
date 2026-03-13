using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Companies;

namespace Promix.Financials.Infrastructure.Persistence.Queries;

public sealed class CurrencyLookupService : ICurrencyLookupService
{
    private readonly PromixDbContext _db;

    public CurrencyLookupService(PromixDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CurrencyOptionDto>> GetActiveCurrenciesAsync(CancellationToken ct = default)
    {
        return await _db.Currencies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .Select(x => new CurrencyOptionDto(
                x.Id,
                x.NameAr,
                x.NameEn))
            .ToListAsync(ct);
    }
}