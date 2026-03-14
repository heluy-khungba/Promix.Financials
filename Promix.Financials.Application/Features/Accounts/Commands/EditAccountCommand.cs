namespace Promix.Financials.Application.Features.Accounts.Commands;

public sealed record EditAccountCommand(
    Guid AccountId,
    Guid CompanyId,
    string ArabicName,
    string? EnglishName,
    bool IsActive,
    string? Notes
);