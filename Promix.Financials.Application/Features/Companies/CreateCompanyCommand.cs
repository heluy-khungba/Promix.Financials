namespace Promix.Financials.Application.Features.Companies;

public sealed record CreateCompanyCommand(
    string Code,
    string Name,
    string BaseCurrency
);