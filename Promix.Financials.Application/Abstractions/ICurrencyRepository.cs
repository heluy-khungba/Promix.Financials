namespace Promix.Financials.Application.Abstractions;

public interface ICurrencyRepository
{
    Task<bool> ExistsActiveAsync(string currencyCode, CancellationToken ct = default);
}