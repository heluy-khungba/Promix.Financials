using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.Accounting;

public sealed class DefaultCurrency : Entity<string>
{

    public DefaultCurrency(
        string code,
        string nameAr,
        string? nameEn,
        string symbol,
        byte decimalPlaces,
        bool isSystem,
        bool isActive,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleException("Currency code is required.");

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessRuleException("Arabic currency name is required.");

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length is < 3 or > 10)
            throw new BusinessRuleException("Currency code length is invalid.");

        if (decimalPlaces > 6)
            throw new BusinessRuleException("Decimal places must be between 0 and 6.");

        Id = normalizedCode;
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        Symbol = string.IsNullOrWhiteSpace(symbol) ? normalizedCode : symbol.Trim();
        DecimalPlaces = decimalPlaces;
        IsSystem = isSystem;
        IsActive = isActive;
        DisplayOrder = displayOrder;
    }

    public string NameAr { get; private set; } = default!;
    public string? NameEn { get; private set; }
    public string Symbol { get; private set; } = default!;
    public byte DecimalPlaces { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    public void Deactivate()
    {
        if (IsSystem)
            throw new BusinessRuleException("System currencies cannot be deactivated.");

        IsActive = false;
    }
}