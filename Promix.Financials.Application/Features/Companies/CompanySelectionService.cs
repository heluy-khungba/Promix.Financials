using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Application.Features.Companies;

public sealed class CompanySelectionService : ICompanySelectionService
{
    private readonly IUserContext _userContext;
    private readonly IUserCompanyRepository _userCompanies;
    private readonly ISessionStore _sessionStore;

    public CompanySelectionService(
        IUserContext userContext,
        IUserCompanyRepository userCompanies,
        ISessionStore sessionStore)
    {
        _userContext = userContext;
        _userCompanies = userCompanies;
        _sessionStore = sessionStore;
    }

    public async Task<IReadOnlyList<CompanySummaryDto>> GetMyCompaniesAsync(CancellationToken ct = default)
    {
        if (!_userContext.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        return await _userCompanies.GetCompaniesForUserAsync(_userContext.UserId, ct);
    }

    public async Task SelectCompanyAsync(Guid companyId, CancellationToken ct = default)
    {
        if (!_userContext.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        var companies = await _userCompanies.GetCompaniesForUserAsync(_userContext.UserId, ct);
        if (!companies.Any(c => c.Id == companyId))
            throw new InvalidOperationException("Company not assigned to this user.");

        var session = await _sessionStore.LoadAsync(ct)
            ?? throw new InvalidOperationException("Session not found.");

        session.CompanyId = companyId;

        // ✅ RememberMe persisted؟
        var activeUserId = await _sessionStore.LoadActiveUserIdAsync(ct);
        var persist = activeUserId.HasValue && activeUserId.Value == session.UserId;

        await _sessionStore.SaveAsync(session, persistent: persist, ct);

        // ✅ تحديث فوري لِـ IUserContext في الذاكرة (بدون Infrastructure reference)
        _userContext.SetCompany(companyId);
    }
}