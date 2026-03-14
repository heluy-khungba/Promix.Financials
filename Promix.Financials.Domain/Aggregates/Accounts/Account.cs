using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Enums;

namespace Promix.Financials.Domain.Aggregates.Accounts;

public sealed class Account : Entity<Guid>
{
    private Account() { } // EF

    public Account(
        Guid companyId, string code, string nameAr, string? nameEn,
        AccountNature nature, bool isPosting, Guid? parentId,
        string? currencyCode, string? systemRole, string? notes, bool isActive)
    {
        if (companyId == Guid.Empty) throw new Exceptions.BusinessRuleException("CompanyId is required.");
        if (string.IsNullOrWhiteSpace(code)) throw new Exceptions.BusinessRuleException("Account code is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new Exceptions.BusinessRuleException("Arabic name is required.");

        Id = Guid.NewGuid();
        CompanyId = companyId;
        Code = code.Trim();
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        Nature = nature;
        IsPosting = isPosting;
        ParentId = parentId;
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? null : currencyCode.Trim().ToUpperInvariant();
        SystemRole = string.IsNullOrWhiteSpace(systemRole) ? null : systemRole.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsSystem = false;
        IsActive = isActive;
    }

    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = default!;
    public string NameAr { get; private set; } = default!;
    public string? NameEn { get; private set; }
    public AccountNature Nature { get; private set; }
    public bool IsPosting { get; private set; }
    public Guid? ParentId { get; private set; }
    public string? CurrencyCode { get; private set; }
    public string? SystemRole { get; private set; }
    public string? Notes { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    // ─── Domain Method: تعديل ────────────────────────���───────────
    /// <summary>
    /// يُعدِّل الحقول المسموح بتعديلها — الكود والطبيعة والعملة ممنوعة.
    /// </summary>
    public void Update(string nameAr, string? nameEn, bool isActive, string? notes)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new Exceptions.BusinessRuleException("Arabic name is required.");

        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        IsActive = isActive;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    // ─── Domain Method: ت��طيل ────────────────────────────────────
    public void Deactivate()
    {
        if (IsSystem)
            throw new Exceptions.BusinessRuleException("System accounts cannot be deactivated.");
        IsActive = false;
    }
}