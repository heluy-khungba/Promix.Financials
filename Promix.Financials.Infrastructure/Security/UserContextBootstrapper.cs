using Promix.Financials.Application.Abstractions;   // ✅ هنا IUserContextBootstrapper + IUserContextBootstrappable

namespace Promix.Financials.Infrastructure.Security;

public sealed class UserContextBootstrapper : IUserContextBootstrapper   // ✅ يعمل لأن using موجود
{
    private readonly ISessionStore _sessionStore;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly IUserContextBootstrappable _userContext;   // ✅ بدلاً من SessionUserContext

    public UserContextBootstrapper(
        ISessionStore sessionStore,
        IUserRepository users,
        IDateTimeProvider clock,
        IUserContextBootstrappable userContext)   // ✅ DI يحقنها تلقائياً — بدون Cast
    {
        _sessionStore = sessionStore;
        _users = users;
        _clock = clock;
        _userContext = userContext;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var session = await _sessionStore.LoadAsync(ct);

        if (session is null || session.IsExpired(_clock.UtcNow))
        {
            await _sessionStore.ClearAsync(ct);
            _userContext.Clear();
            return;
        }

        var user = await _users.FindByIdAsync(session.UserId, ct);
        if (user is null)
        {
            await _sessionStore.ClearAsync(ct);
            _userContext.Clear();
            return;
        }

        _userContext.SetSession(session, user.Username);
    }
}