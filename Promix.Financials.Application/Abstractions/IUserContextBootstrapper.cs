namespace Promix.Financials.Application.Abstractions;

/// <summary>
/// يسمح بتهيئة ومسح IUserContext من Infrastructure بدون cast مباشر.
/// </summary>
public interface IUserContextBootstrappable : IUserContext
{
    void SetSession(AppSession session, string username);
    void Clear();
}