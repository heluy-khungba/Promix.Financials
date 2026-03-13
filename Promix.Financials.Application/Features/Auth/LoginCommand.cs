using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promix.Financials.Application.Features.Auth;

public sealed record LoginCommand(
    string Username,
    string Password,
    bool RememberMe
);