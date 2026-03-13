using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promix.Financials.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
