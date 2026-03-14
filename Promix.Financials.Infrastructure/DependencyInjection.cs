using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Application.Features.Accounts.Queries;
using Promix.Financials.Application.Features.Accounts.Services;  // 🆕
using Promix.Financials.Application.Features.Auth;
using Promix.Financials.Application.Features.Companies;
using Promix.Financials.Infrastructure.Persistence;
using Promix.Financials.Infrastructure.Persistence.Queries;
using Promix.Financials.Infrastructure.Persistence.Repositories;
using Promix.Financials.Infrastructure.Security;

namespace Promix.Financials.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PromixDbContext>(opt => opt.UseSqlServer(connectionString));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<PromixDbContext>());

        // Security
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Repositories
        services.AddScoped<IAccountRepository, EfAccountRepository>();
        services.AddScoped<ICompanyAdminRepository, EfCompanyAdminRepository>();
        services.AddScoped<ICurrencyRepository, EfCurrencyRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserCompanyRepository, EfUserCompanyRepository>();

        // Queries
        services.AddScoped<IChartOfAccountsQuery, ChartOfAccountsQuery>();

        // Services — Application
        services.AddScoped<CreateAccountService>();
        services.AddScoped<EditAccountService>();    // 🆕
        services.AddScoped<DeleteAccountService>();  // 🆕
        services.AddScoped<CreateCompanyService>();
        services.AddScoped<ICompanySelectionService, CompanySelectionService>();
        services.AddScoped<ICompanyInitializer,
            Promix.Financials.Infrastructure.Persistence.Seeding.CompanyInitializer>();
        services.AddScoped<ICurrencyLookupService, CurrencyLookupService>();

        // UserContext + Auth
        services.AddSingleton<IUserContext, SessionUserContext>();
        services.AddSingleton<IUserContextBootstrapper, UserContextBootstrapper>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}