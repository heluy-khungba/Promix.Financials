namespace Promix.Financials.Domain.Security;

public sealed class UserCompany
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }

    private UserCompany() { }

    public UserCompany(Guid userId, Guid companyId)
    {
        UserId = userId;
        CompanyId = companyId;
    }
}