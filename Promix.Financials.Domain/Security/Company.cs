using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.Security;

public sealed class Company : Entity<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string BaseCurrency { get; private set; } = "USD";
    public bool IsActive { get; private set; } = true;

    private Company() { }

    public Company(string code, string name, string baseCurrency)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleException("Company code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("Company name is required.");

        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new BusinessRuleException("Base currency is required.");

        Id = Guid.NewGuid(); // <-- هذا هو الإصلاح الأهم

        Code = code.Trim();
        Name = name.Trim();
        BaseCurrency = baseCurrency.Trim().ToUpperInvariant();
    }

    public void Deactivate() => IsActive = false;
}