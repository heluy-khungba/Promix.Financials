using Promix.Financials.Application.Features.Companies;

namespace Promix.Financials.Application.Abstractions;

public interface ICurrencyLookupService
{
    Task<IReadOnlyList<CurrencyOptionDto>> GetActiveCurrenciesAsync(CancellationToken ct = default);
}