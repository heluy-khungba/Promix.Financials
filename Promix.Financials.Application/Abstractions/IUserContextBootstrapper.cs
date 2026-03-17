namespace Promix.Financials.Application.Abstractions;

public interface IUserContextBootstrapper
{
    Task InitializeAsync(CancellationToken ct = default);
}
