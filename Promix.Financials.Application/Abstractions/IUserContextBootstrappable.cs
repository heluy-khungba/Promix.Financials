namespace Promix.Financials.Application.Abstractions;

/// <summary>
/// Allows Infrastructure to hydrate and clear the in-memory user context
/// without depending on the concrete implementation.
/// </summary>
public interface IUserContextBootstrappable : IUserContext
{
    void SetSession(AppSession session, string username);
    void Clear();
}
