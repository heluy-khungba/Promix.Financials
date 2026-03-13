using Promix.Financials.Application.Abstractions;

namespace Promix.Financials.Application.Features.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IDateTimeProvider _clock;
    private readonly ISessionStore _sessionStore;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        IDateTimeProvider clock,
        ISessionStore sessionStore)
    {
        _users = users;
        _hasher = hasher;
        _clock = clock;
        _sessionStore = sessionStore;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        var username = (command.Username ?? string.Empty).Trim();
        var password = command.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return LoginResult.Failed("UsernameOrPasswordRequired");

        var user = await _users.FindByUsernameAsync(username, ct);
        if (user is null)
            return LoginResult.Failed("InvalidCredentials");

        if (!_hasher.Verify(password, user.PasswordHash))
            return LoginResult.Failed("InvalidCredentials");

        var now = _clock.UtcNow;

        var expires = command.RememberMe
            ? now.AddDays(30)
            : now.AddHours(12);

        var session = new AppSession
        {
            UserId = user.Id,
            CompanyId = null,
            CreatedAtUtc = now,
            ExpiresAtUtc = expires
        };

        // ✅ حفظ الجلسة (persistent فقط إذا RememberMe)
        await _sessionStore.SaveAsync(session, persistent: command.RememberMe, ct);

        // UX فقط
        await _sessionStore.SaveLastUsernameAsync(username, ct);

        return LoginResult.Success(user.Id, selectedCompanyId: null);
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        // يمسح الجلسة runtime + persisted للمستخدم النشط
        await _sessionStore.ClearAsync(ct);
    }
}