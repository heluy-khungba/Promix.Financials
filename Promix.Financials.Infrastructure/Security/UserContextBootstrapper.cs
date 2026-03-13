using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Infrastructure.Security;

public sealed class UserContextBootstrapper : IUserContextBootstrapper
{
    private readonly ISessionStore _sessionStore;
    private readonly IUserRepository _users;
    private readonly IDateTimeProvider _clock;
    private readonly SessionUserContext _userContext;

    public UserContextBootstrapper(
        ISessionStore sessionStore,
        IUserRepository users,
        IDateTimeProvider clock,
        IUserContext userContext)
    {
        _sessionStore = sessionStore;
        _users = users;
        _clock = clock;
        _userContext = (SessionUserContext)userContext;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var session = await _sessionStore.LoadAsync(ct);

        if (session is null || session.IsExpired(_clock.UtcNow))
        {
            // clears active user's persisted session if any + runtime
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