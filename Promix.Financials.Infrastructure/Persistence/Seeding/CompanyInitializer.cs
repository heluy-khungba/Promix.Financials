using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Domain.Enums;
using Promix.Financials.Domain.Security;
using Promix.Financials.Infrastructure.Persistence;

namespace Promix.Financials.Infrastructure.Persistence.Seeding;

public sealed class CompanyInitializer : ICompanyInitializer
{
    private readonly PromixDbContext _db;

    public CompanyInitializer(PromixDbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync(Guid companyId, CancellationToken ct = default)
    {
        // ✅ لا تزرع إذا موجود حسابات
        var hasAccounts = await _db.Set<Account>().AnyAsync(a => a.CompanyId == companyId, ct);
        if (hasAccounts) return;

       

        var template = DefaultChartOfAccountsTemplate.FromPdfV1();

        var created = new Dictionary<string, Account>();

        // نخلق بالترتيب من الأقصر للأطول لضمان وجود الأب أولاً
        foreach (var node in template.OrderBy(x => x.Code.Length).ThenBy(x => x.Code))
        {
            var parentCode = FindParentCode(node.Code, created.Keys);
            Guid? parentId = parentCode is null ? null : created[parentCode].Id;

            var acc = new Account(
    companyId: companyId,
    code: node.Code,
    nameAr: node.NameAr,
    nameEn: null,
    nature: node.Nature,
    isPosting: node.IsPosting,
    parentId: parentId,
    currencyCode: null,
    systemRole: null,
    notes: null,
    isActive: true
);

            _db.Set<Account>().Add(acc);
            created[node.Code] = acc;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static string? FindParentCode(string code, IEnumerable<string> createdCodes)
    {
        string? best = null;

        foreach (var c in createdCodes)
        {
            if (c.Length >= code.Length) continue;

            if (code.StartsWith(c, StringComparison.Ordinal) &&
                (best is null || c.Length > best.Length))
            {
                best = c;
            }
        }

        return best;
    }

    private sealed record TemplateNode(string Code, string NameAr, AccountNature Nature, bool IsPosting);

    private static class DefaultChartOfAccountsTemplate
    {
        // ✅ هذه V1 مبنية من PDF الذي رفعته (الأكواد الرئيسية + أشهر البنود)
        // لاحقاً نوسعها/نستبدلها بـ JSON Template بدون تغيير أي مكان آخر.
        public static List<TemplateNode> FromPdfV1() => new()
        {
            new("1",  "الموجودات", AccountNature.Debit,  false),
            new("11", "الموجودات الثابتة", AccountNature.Debit, false),
            new("111","الأراضي", AccountNature.Debit, true),
            new("112","عقارات", AccountNature.Debit, true),
            new("113","أثاث ومفروشات", AccountNature.Debit, true),
            new("114","سيارات", AccountNature.Debit, true),

            new("12", "الموجودات المتداولة", AccountNature.Debit, false),
            new("121","الزبائن", AccountNature.Debit, false),
            new("122","مدينون مختلفون", AccountNature.Debit, true),
            new("123","مسحوبات الشركاء", AccountNature.Debit, false),
            new("124","المخزون", AccountNature.Debit, false),
            new("1241","مخزون بضاعة جاهزة آخر المدة", AccountNature.Debit, true),

            new("13","الأموال الجاهزة", AccountNature.Debit, false),
            new("131","الصندوق", AccountNature.Debit, false),
            new("132","المصرف", AccountNature.Debit, false),

            new("2",  "المطاليب", AccountNature.Credit, false),
            new("21", "المطاليب الثابتة", AccountNature.Credit, false),
            new("211","رأس المال", AccountNature.Credit, false),
            new("212","القروض", AccountNature.Credit, true),
            new("22", "المطاليب المتداولة", AccountNature.Credit, false),
            new("221","الموردون", AccountNature.Credit, false),

            new("3",  "صافي المشتريات", AccountNature.Debit, false),
            new("31", "المشتريات", AccountNature.Debit, true),

            new("4",  "صافي المبيعات", AccountNature.Credit, false),
            new("41", "المبيعات", AccountNature.Credit, true),
            new("42", "مرتجع المبيعات", AccountNature.Debit, true),
            new("43", "الحسم الممنوح", AccountNature.Debit, true),

            new("5",  "المصاريف", AccountNature.Debit, false),
            new("501","رواتب وأجور", AccountNature.Debit, true),
            new("502","كهرباء وماء", AccountNature.Debit, true),
            new("503","هاتف وفاكس وإنترنت", AccountNature.Debit, true),
            new("504","إكراميات وهدايا", AccountNature.Debit, true),
            new("505","نقل وانتقال", AccountNature.Debit, true),
            new("506","وقود ومحروقات", AccountNature.Debit, true),
            new("507","صيانة وقطع غيار", AccountNature.Debit, true),
            new("508","قرطاسية ومطبوعات", AccountNature.Debit, true),
            new("509","زيوت وشحوم", AccountNature.Debit, true),
            new("510","مصاريف متفرقة", AccountNature.Debit, true),

            new("6",  "إيرادات", AccountNature.Credit, false),
            new("601","إيرادات مختلفة", AccountNature.Credit, true),

            new("7",  "البضاعة", AccountNature.Debit, false),
            new("71", "بضاعة أول المدة", AccountNature.Debit, true),
            new("72", "بضاعة آخر المدة", AccountNature.Debit, true),

            new("00", "الميزانية", AccountNature.Debit, false),
            new("01", "الأرباح والخسائر", AccountNature.Credit, false),
            new("02", "المتاجرة", AccountNature.Debit, false),
        };
    }
}