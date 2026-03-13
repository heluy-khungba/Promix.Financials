using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Application.Features.Companies;

public sealed class CreateCompanyService
{
    private readonly IUserContext _userContext;
    private readonly ICompanyAdminRepository _companies;
    private readonly ICompanyInitializer _initializer;
    private readonly ICurrencyRepository _currencies;
    public CreateCompanyService(
    IUserContext userContext,
    ICompanyAdminRepository companies,
    ICompanyInitializer initializer,
    ICurrencyRepository currencies)
    {
        _userContext = userContext;
        _companies = companies;
        _initializer = initializer;
        _currencies = currencies;
    }

    public async Task<CreateCompanyResult> CreateAsync(CreateCompanyCommand cmd, CancellationToken ct = default)
    {
        if (!_userContext.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        var code = cmd.Code.Trim();
        var name = cmd.Name.Trim();
        var baseCurrency = cmd.BaseCurrency.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(baseCurrency))
            throw new InvalidOperationException("Base currency is required.");

        if (!await _currencies.ExistsActiveAsync(baseCurrency, ct))
            throw new InvalidOperationException("Base currency is invalid or inactive.");

        if (await _companies.CompanyCodeExistsAsync(code, ct))
            throw new InvalidOperationException("Company code already exists.");

        var company = await _companies.CreateCompanyAsync(
            code: code,
            name: name,
            baseCurrency: baseCurrency,
            ownerUserId: _userContext.UserId,
            ct: ct);

        await _initializer.InitializeAsync(company.Id, ct);

        return new CreateCompanyResult(company.Id);
    }
}