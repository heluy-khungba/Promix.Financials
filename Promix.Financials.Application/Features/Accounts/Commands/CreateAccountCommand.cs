using Promix.Financials.Domain.Enums;

namespace Promix.Financials.Application.Features.Accounts.Commands;

public sealed record CreateAccountCommand(
    Guid CompanyId,
    Guid? ParentId,
    string Code,
    string ArabicName,
    string? EnglishName,
    bool IsPosting,
    AccountNature Nature,
    string? CurrencyCode,
    string? SystemRole,
    bool IsActive,
    string? Notes
);