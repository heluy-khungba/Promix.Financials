using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.Accounting;

public sealed class CurrencyRate : Entity<Guid>
{
    private CurrencyRate() { }

    public CurrencyRate(
        Guid companyId,
        string currencyCode,
        DateOnly rateDate,
        decimal rate,
        string? source,
        string? notes)
    {
        if (companyId == Guid.Empty)
            throw new BusinessRuleException("CompanyId is required.");

        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new BusinessRuleException("Currency code is required.");

        if (rate <= 0)
            throw new BusinessRuleException("Exchange rate must be greater than zero.");

        Id = Guid.NewGuid();
        CompanyId = companyId;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        RateDate = rateDate;
        Rate = decimal.Round(rate, 8, MidpointRounding.AwayFromZero);
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid CompanyId { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public DateOnly RateDate { get; private set; }
    public decimal Rate { get; private set; }
    public string? Source { get; private set; }
    public string? Notes { get; private set; }
}