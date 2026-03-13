using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Domain.Security;
using Promix.Financials.Domain.Accounting;
namespace Promix.Financials.Infrastructure.Persistence.Seeding;

public static class SeedData
{
    public static async Task EnsureSeedAsync(PromixDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();
        if (!await db.Currencies.AnyAsync())
        {
            db.Currencies.AddRange(
    new Currency("USD", "دولار أمريكي", "US Dollar", "$", 2, true, true, 1),
    new Currency("EUR", "يورو", "Euro", "€", 2, true, true, 2),
    new Currency("SAR", "ريال سعودي", "Saudi Riyal", "ر.س", 2, true, true, 3),
    new Currency("AED", "درهم إماراتي", "UAE Dirham", "د.إ", 2, true, true, 4),
    new Currency("IQD", "دينار عراقي", "Iraqi Dinar", "د.ع", 2, true, true, 5),
    new Currency("TRY", "ليرة تركية", "Turkish Lira", "₺", 2, true, true, 6),
    new Currency("SYP", "ليرة سورية", "Syrian Pound", "£S", 2, true, true, 7)
);

            await db.SaveChangesAsync();
        }
        var hasSyp = await db.Currencies.AnyAsync(x => x.Id == "SYP");
        if (!hasSyp)
        {
            db.Currencies.Add(new Currency("SYP", "ليرة سورية", "Syrian Pound", "£S", 2, true, true, 7));
            await db.SaveChangesAsync();
        }
        // 1) Company MAIN
        var company = await db.Companies.FirstOrDefaultAsync(x => x.Code == "MAIN");
        if (company is null)
        {
            company = new Company("MAIN", "Main Company", "USD");
            db.Companies.Add(company);
            await db.SaveChangesAsync();
        }

        // ✅ 1b) Company BR2 (شركة ثانية للتجربة)
        var company2 = await db.Companies.FirstOrDefaultAsync(x => x.Code == "BR2");
        if (company2 is null)
        {
            company2 = new Company("BR2", "Branch 2", "USD");
            db.Companies.Add(company2);
            await db.SaveChangesAsync();
        }

        // 2) Role
        var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.Name == "Admin");
        if (adminRole is null)
        {
            adminRole = new Role("Admin", isSystem: true);
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
        }

        // 3) User
        var admin = await db.Users.FirstOrDefaultAsync(x => x.Username == "admin");
        if (admin is null)
        {
            admin = new User("admin", hasher.Hash("Admin@123"));
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        // 4) UserCompany link (MAIN)
        var hasCompanyLink = await db.UserCompanies.AnyAsync(x => x.UserId == admin.Id && x.CompanyId == company.Id);
        if (!hasCompanyLink)
        {
            db.UserCompanies.Add(new UserCompany(admin.Id, company.Id));
            await db.SaveChangesAsync();
        }

        // ✅ 4b) UserCompany link (BR2)
        var hasCompany2Link = await db.UserCompanies.AnyAsync(x => x.UserId == admin.Id && x.CompanyId == company2.Id);
        if (!hasCompany2Link)
        {
            db.UserCompanies.Add(new UserCompany(admin.Id, company2.Id));
            await db.SaveChangesAsync();
        }

        // 5) UserRole link
        var hasRoleLink = await db.UserRoles.AnyAsync(x => x.UserId == admin.Id && x.RoleId == adminRole.Id);
        if (!hasRoleLink)
        {
            db.UserRoles.Add(new UserRole(admin.Id, adminRole.Id));
            await db.SaveChangesAsync();
        }
    }
}