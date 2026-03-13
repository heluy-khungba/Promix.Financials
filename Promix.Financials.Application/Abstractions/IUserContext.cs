using System;

namespace Promix.Financials.Application.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }
    Guid? CompanyId { get; }     // ✅ nullable
    string Username { get; }
    bool IsAuthenticated { get; }

    // ✅ Guard
    bool HasCompanySelected => CompanyId is not null;

    // ✅ مهم: تحديث الشركة المختارة في الذاكرة بدون اعتماد على Infrastructure
    void SetCompany(Guid? companyId);
}