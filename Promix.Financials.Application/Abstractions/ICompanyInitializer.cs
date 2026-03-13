namespace Promix.Financials.Application.Abstractions;

public interface ICompanyInitializer
{
    Task InitializeAsync(Guid companyId, CancellationToken ct = default);
}