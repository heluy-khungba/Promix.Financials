using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Infrastructure.Security;

public sealed class SessionUserContext : IUserContextBootstrappable   // ✅ كان IUserContext
{
    public Guid UserId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public string Username { get; private set; } = string.Empty;

    public bool IsAuthenticated => UserId != Guid.Empty;

    public void SetSession(AppSession session, string username)
    {
        UserId = session.UserId;
        CompanyId = session.CompanyId;
        Username = username ?? string.Empty;
    }

    public void SetCompany(Guid? companyId)
    {
        CompanyId = companyId;
    }

    public void Clear()
    {
        UserId = Guid.Empty;
        CompanyId = null;
        Username = string.Empty;
    }
}