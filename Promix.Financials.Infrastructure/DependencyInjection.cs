using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Promix.Financials.Application.Abstractions;                    // ✅ هنا IUserContextBootstrapper + IUserContextBootstrappable
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Application.Features.Accounts.Queries;
using Promix.Financials.Application.Features.Accounts.Services;
using Promix.Financials.Application.Features.Auth;
using Promix.Financials.Application.Features.Companies;
using Promix.Financials.Application.Features.Currencies.Queries;
using Promix.Financials.Application.Features.Currencies.Services;
using Promix.Financials.Application.Features.Journals.Queries;
using Promix.Financials.Application.Features.Journals.Services;
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
        services.AddScoped<IJournalEntryRepository, EfJournalEntryRepository>();
        services.AddScoped<ICompanyAdminRepository, EfCompanyAdminRepository>();
        services.AddScoped<ICompanyCurrencyRepository, EfCompanyCurrencyRepository>();
        services.AddScoped<ICurrencyRepository, EfCurrencyRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IUserCompanyRepository, EfUserCompanyRepository>();

        // Queries
        services.AddScoped<IChartOfAccountsQuery, ChartOfAccountsQuery>();
        services.AddScoped<IJournalEntriesQuery, JournalEntriesQuery>();

        // Services — Application
        services.AddScoped<CreateAccountService>();
        services.AddScoped<EditAccountService>();
        services.AddScoped<DeleteAccountService>();
        services.AddScoped<CreateJournalEntryService>();
        services.AddScoped<PostJournalEntryService>();
        services.AddScoped<CreateCompanyService>();
        services.AddScoped<ICompanySelectionService, CompanySelectionService>();
        services.AddScoped<ICompanyInitializer,
            Promix.Financials.Infrastructure.Persistence.Seeding.CompanyInitializer>();
        services.AddScoped<ICurrencyLookupService, CurrencyLookupService>();

        // Currency Services
        services.AddScoped<CreateCompanyCurrencyService>();
        services.AddScoped<EditCompanyCurrencyService>();
        services.AddScoped<DeactivateCompanyCurrencyService>();
        services.AddScoped<CompanyCurrenciesQuery>();

        // ✅ UserContext — تسجيل واحد + 3 interfaces تشير لنفس الكائن
        services.AddSingleton<SessionUserContext>();
        services.AddSingleton<IUserContext>(sp => sp.GetRequiredService<SessionUserContext>());
        services.AddSingleton<IUserContextBootstrappable>(sp => sp.GetRequiredService<SessionUserContext>());

        // ✅ Bootstrapper — يستقبل IUserContextBootstrappable من DI بدون Cast
        services.AddSingleton<IUserContextBootstrapper, UserContextBootstrapper>();

        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
