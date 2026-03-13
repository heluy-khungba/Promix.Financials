using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Auth;
using Promix.Financials.Infrastructure.Persistence;
using Promix.Financials.Infrastructure.Security;
using Promix.Financials.Application.Features.Companies;
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Infrastructure.Persistence.Queries;
using Promix.Financials.Infrastructure.Persistence.Repositories;

namespace Promix.Financials.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PromixDbContext>(opt => opt.UseSqlServer(connectionString));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<PromixDbContext>());
        services.AddScoped<ICompanySelectionService, CompanySelectionService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IChartOfAccountsQuery, ChartOfAccountsQuery>();
        services.AddScoped<ICompanyAdminRepository, EfCompanyAdminRepository>();
        services.AddScoped<ICompanyInitializer, Promix.Financials.Infrastructure.Persistence.Seeding.CompanyInitializer>();
        services.AddScoped<IAccountRepository, EfAccountRepository>();
        services.AddScoped<ICurrencyRepository, EfCurrencyRepository>();
        services.AddScoped<CreateAccountService>();
        services.AddScoped<CreateCompanyService>();
        services.AddScoped<ICurrencyLookupService, CurrencyLookupService>();
        // UserContext + Bootstrapper
        services.AddSingleton<IUserContext, SessionUserContext>();
        services.AddSingleton<IUserContextBootstrapper, UserContextBootstrapper>();

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        // EF
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserCompanyRepository, EfUserCompanyRepository>();
        return services;
    }
}