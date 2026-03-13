using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Promix.Financials.Application.Features.Accounts;

namespace Promix.Financials.UI.ViewModels.Accounts;

public sealed class NewAccountDialogViewModel : INotifyPropertyChanged
{
    private readonly IChartOfAccountsQuery _query;

    public ObservableCollection<ParentAccountOptionVm> ParentAccounts { get; } = new();
    public ObservableCollection<string> AccountTypes { get; } = new() { "Group", "Postable" };

    // لاحقاً: العملات تُقرأ من إعدادات الشركة + BaseCurrency
    public ObservableCollection<string> Currencies { get; } = new() { "USD", "IQD", "EUR" };

    // لاحقاً: enum + Localization
    public ObservableCollection<string> SystemRoles { get; } = new()
    {
        "", // None
        "CashMain","CashDaily","BankMain","ARControl","APControl","InventoryControl","COGS",
        "SalesRevenue","SalesReturns","SalesDiscountAllowed","InventoryAdjustments","RetainedEarnings"
    };

    private Guid _companyId = Guid.Empty;

    // cache of flat accounts for code suggestion / category derivation
    private List<AccountFlatDto> _flat = new();

    public NewAccountDialogViewModel(IChartOfAccountsQuery query)
    {
        _query = query;

        // defaults
        _selectedAccountType = "Postable";
        _selectedCurrency = "USD";
        _selectedSystemRole = "";
        _isActive = true;

        // لا نضيف Placeholder هنا بعد الآن
        ParentAccounts.Clear();
        ParentAccounts.Add(new ParentAccountOptionVm(null, "(Root)", ""));
        SelectedParentAccount = ParentAccounts.FirstOrDefault();
        RecomputeSuggestedCode();
    }

    /// <summary>
    /// يجب استدعاؤها قبل فتح الديالوج لملء ParentAccounts من DB
    /// </summary>
    public async Task InitializeAsync(Guid companyId)
    {
        _companyId = companyId;

        var rows = await _query.GetAccountsAsync(companyId);

        // فقط الحسابات التجميعية (غير حركية) كـ Parents
        _flat = rows.ToList();

        var parents = rows
            .Where(a => !a.IsPosting) // Group accounts only
            .OrderBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ParentAccounts.Clear();
        ParentAccounts.Add(new ParentAccountOptionVm(null, "(Root)", ""));

        // عرض هرمي بسيط بالـ indent حسب عمق الكود
        foreach (var p in parents)
        {
            var indent = GetIndentFromCode(p.Code);
            var display = $"{indent}{p.Code} - {p.ArabicName}";
            ParentAccounts.Add(new ParentAccountOptionVm(p.Id, display, p.Code));
        }

        // حاول تعيين Root افتراضياً
        SelectedParentAccount = ParentAccounts.FirstOrDefault();
        RecomputeSuggestedCode();
    }

    private static string GetIndentFromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "";
        var depth = code.Count(c => c == '.'); // 0 => root group, 1 => child, ...
        return new string(' ', depth * 2);
    }

    private ParentAccountOptionVm? _selectedParentAccount;
    public ParentAccountOptionVm? SelectedParentAccount
    {
        get => _selectedParentAccount;
        set
        {
            if (_selectedParentAccount == value) return;
            _selectedParentAccount = value;
            OnPropertyChanged();

            RecomputeSuggestedCode();
            RecomputeDerivedCategory();
        }
    }

    private string _suggestedCode = "";
    public string SuggestedCode
    {
        get => _suggestedCode;
        private set { if (_suggestedCode == value) return; _suggestedCode = value; OnPropertyChanged(); }
    }

    private string _arabicName = "";
    public string ArabicName
    {
        get => _arabicName;
        set { if (_arabicName == value) return; _arabicName = value; OnPropertyChanged(); ValidateArabicName(); }
    }

    private string? _englishName;
    public string? EnglishName
    {
        get => _englishName;
        set { if (_englishName == value) return; _englishName = value; OnPropertyChanged(); }
    }

    private string _selectedAccountType;
    public string SelectedAccountType
    {
        get => _selectedAccountType;
        set { if (_selectedAccountType == value) return; _selectedAccountType = value; OnPropertyChanged(); }
    }

    private string _selectedCurrency;
    public string SelectedCurrency
    {
        get => _selectedCurrency;
        set { if (_selectedCurrency == value) return; _selectedCurrency = value; OnPropertyChanged(); }
    }

    private string _selectedSystemRole;
    public string SelectedSystemRole
    {
        get => _selectedSystemRole;
        set { if (_selectedSystemRole == value) return; _selectedSystemRole = value; OnPropertyChanged(); }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set { if (_isActive == value) return; _isActive = value; OnPropertyChanged(); }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set { if (_notes == value) return; _notes = value; OnPropertyChanged(); }
    }

    // Validation
    private bool _isArabicNameValid = true;
    public bool IsArabicNameValid
    {
        get => _isArabicNameValid;
        private set { if (_isArabicNameValid == value) return; _isArabicNameValid = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); }
    }

    private string _arabicNameError = "";
    public string ArabicNameError
    {
        get => _arabicNameError;
        private set { if (_arabicNameError == value) return; _arabicNameError = value; OnPropertyChanged(); }
    }

    public bool CanSubmit => IsArabicNameValid && !string.IsNullOrWhiteSpace(ArabicName);

    public void Validate() => ValidateArabicName();

    private void ValidateArabicName()
    {
        if (string.IsNullOrWhiteSpace(ArabicName))
        {
            IsArabicNameValid = false;
            ArabicNameError = "Arabic name is required.";
            return;
        }

        IsArabicNameValid = true;
        ArabicNameError = "";
    }

    // Dialog output
    public NewAccountDraftVm BuildDraft()
        => new(
            CompanyId: _companyId,
            ParentId: SelectedParentAccount?.Id,
            Code: SuggestedCode,
            ArabicName: ArabicName.Trim(),
            EnglishName: string.IsNullOrWhiteSpace(EnglishName) ? null : EnglishName.Trim(),
            IsPosting: SelectedAccountType == "Postable",
            CurrencyCode: string.IsNullOrWhiteSpace(SelectedCurrency) ? null : SelectedCurrency,
            SystemRole: string.IsNullOrWhiteSpace(SelectedSystemRole) ? null : SelectedSystemRole,
            Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            IsActive: IsActive
        );

    private void RecomputeSuggestedCode()
    {
        var parent = SelectedParentAccount;
        var parentId = parent?.Id;
        var parentCode = parent?.Code ?? "";

        if (_companyId == Guid.Empty || _flat.Count == 0)
        {
            // في حال لم يتم InitializeAsync بعد
            SuggestedCode = string.IsNullOrWhiteSpace(parentCode) ? "1" : $"{parentCode}.1";
            return;
        }

        // siblings = نفس ParentId
        var siblings = _flat
            .Where(a => a.ParentId == parentId)
            .Select(a => a.Code)
            .ToList();

        var next = ComputeNextSegment(siblings, parentCode);

        SuggestedCode = string.IsNullOrWhiteSpace(parentCode)
            ? next.ToString()
            : $"{parentCode}.{next}";
    }

    private static int ComputeNextSegment(List<string> siblingCodes, string parentCode)
    {
        // نستخرج آخر جزء رقمي بعد آخر "."
        // مثال: parent=1.1 => siblings: 1.1.1, 1.1.2 => next=3
        // مثال: parent=root => siblings: 1,2,3 => next=4
        var max = 0;

        foreach (var code in siblingCodes)
        {
            if (string.IsNullOrWhiteSpace(code)) continue;

            // تأكد أنه فعلاً تحت نفس parent code عندما parentCode غير فارغ
            if (!string.IsNullOrWhiteSpace(parentCode))
            {
                if (!code.StartsWith(parentCode + ".", StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var lastSegment = code.Split('.').LastOrDefault();
            if (int.TryParse(lastSegment, out var n))
                max = Math.Max(max, n);
        }

        return max + 1;
    }

    private void RecomputeDerivedCategory()
    {
        // مبدئياً من prefix (كما كان عندك)
        var code = SelectedParentAccount?.Code ?? "";

        DerivedCategoryText = code switch
        {
            "" => "Root (auto later)",
            var c when c.StartsWith("1") => "Assets",
            var c when c.StartsWith("2") => "Liabilities",
            var c when c.StartsWith("3") => "Equity",
            var c when c.StartsWith("4") => "Revenue",
            var c when c.StartsWith("5") => "Expenses",
            _ => "—"
        };
    }

    private string _derivedCategoryText = "—";
    public string DerivedCategoryText
    {
        get => _derivedCategoryText;
        private set { if (_derivedCategoryText == value) return; _derivedCategoryText = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed record ParentAccountOptionVm(Guid? Id, string DisplayName, string Code);

public sealed record NewAccountDraftVm(
    Guid CompanyId,
    Guid? ParentId,
    string Code,
    string ArabicName,
    string? EnglishName,
    bool IsPosting,
    string? CurrencyCode,
    string? SystemRole,
    string? Notes,
    bool IsActive
);