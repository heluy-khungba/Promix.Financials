using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Promix.Financials.Domain.Security;

namespace Promix.Financials.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);

    // ✅ لإعادة بناء IUserContext من Session (بدون تخزين Username داخل Session)
    Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default);
}