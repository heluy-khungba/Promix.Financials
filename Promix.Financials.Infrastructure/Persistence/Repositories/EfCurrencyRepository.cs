using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Infrastructure.Persistence.Repositories;

public sealed class EfCurrencyRepository : ICurrencyRepository
{
    private readonly PromixDbContext _db;

    public EfCurrencyRepository(PromixDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsActiveAsync(string currencyCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return Task.FromResult(false);

        var normalized = currencyCode.Trim().ToUpperInvariant();

        return _db.Currencies.AnyAsync(x => x.Id == normalized && x.IsActive, ct);
    }
}