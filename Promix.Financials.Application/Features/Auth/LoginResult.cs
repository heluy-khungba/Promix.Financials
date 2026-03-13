namespace Promix.Financials.Application.Features.Auth;

public sealed record LoginResult(
    bool Succeeded,
    Guid? UserId,
    Guid? SelectedCompanyId,
    string? ErrorCode)
{
    public static LoginResult Failed(string errorCode)
        => new(false, null, null, errorCode);

    public static LoginResult Success(Guid userId, Guid? selectedCompanyId)
        => new(true, userId, selectedCompanyId, null);
}