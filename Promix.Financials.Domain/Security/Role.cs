using Promix.Financials.Domain.Common;

namespace Promix.Financials.Domain.Security;

public sealed class Role : Entity<Guid>
{
    public string Name { get; private set; } = default!; // Admin, Accountant, ...
    public bool IsSystem { get; private set; } = false;

    private Role() { }

    public Role(string name, bool isSystem = false)
    {
        Name = name.Trim();
        IsSystem = isSystem;
    }
}