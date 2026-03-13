namespace Promix.Financials.Application.Abstractions;

public interface ISessionStore
{
    // Runtime session (in-memory, valid while app is running)
    Task<AppSession?> LoadAsync(CancellationToken ct = default);

    // Save runtime session always; persist it only when persistent=true (RememberMe)
    Task SaveAsync(AppSession session, bool persistent, CancellationToken ct = default);

    // Clears current user's session (runtime + persisted if exists)
    Task ClearAsync(CancellationToken ct = default);

    // Active persisted user (used on startup to restore RememberMe sessions)
    Task<Guid?> LoadActiveUserIdAsync(CancellationToken ct = default);
    Task SetActiveUserIdAsync(Guid? userId, CancellationToken ct = default);

    // UX only
    Task<string?> LoadLastUsernameAsync(CancellationToken ct = default);
    Task SaveLastUsernameAsync(string? username, CancellationToken ct = default);
}