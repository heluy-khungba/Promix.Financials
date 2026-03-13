using Promix.Financials.Domain.Common;

namespace Promix.Financials.Domain.Security;

public sealed class User : Entity<Guid>
{
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(string username, string passwordHash)
    {
        Username = username.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }

    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void Deactivate() => IsActive = false;
}