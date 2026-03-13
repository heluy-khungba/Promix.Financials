using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promix.Financials.Application.Abstractions;

public interface IUserContextBootstrapper
{
    Task InitializeAsync(CancellationToken ct = default);
}