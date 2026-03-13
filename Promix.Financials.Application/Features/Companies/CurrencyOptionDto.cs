namespace Promix.Financials.Application.Features.Companies;

public sealed record CurrencyOptionDto(
    string Code,
    string ArabicName,
    string? EnglishName)
{
    public string DisplayName =>
        $"{Code} - {ArabicName}";
}