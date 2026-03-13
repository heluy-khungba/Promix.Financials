using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Application.Features.Companies;

namespace Promix.Financials.UI.ViewModels.Accounts;

public sealed class NewAccountDialogViewModel : INotifyPropertyChanged
{
    private readonly IChartOfAccountsQuery _query;
    private readonly ICurrencyLookupService _currencyLookup;

    public ObservableCollection<ParentAccountOptionVm> ParentAccounts { get; } = new();
    public ObservableCollection<string> AccountTypes { get; } = new() { "Group", "Postable" };

    // ✅ العملات تُقرأ من DB عبر ICurrencyLookupService
    public ObservableCollection<CurrencyOptionDto> Currencies { get; } = new();

    public ObservableCollection<string> SystemRoles { get; } = new()
    {
        "", // None
        "CashMain","CashDaily","BankMain","ARControl","APControl","InventoryControl","COGS",
        "SalesRevenue","SalesReturns","SalesDiscountAllowed","InventoryAdjustments","RetainedEarnings"
    };

    private Guid _companyId = Guid.Empty;
    private List<AccountFlatDto> _flat = new();

    public NewAccountDialogViewModel(
        IChartOfAccountsQuery query,
        ICurrencyLookupService currencyLookup)
    {
        _query = query;
        _currencyLookup = currencyLookup;

        // defaults
        _selectedAccountType = "Postable";
        _selectedCurrency = null;
        _selectedSystemRole = "";
        _isActive = true;

        ParentAccounts.Clear();
        ParentAccounts.Add(new ParentAccountOptionVm(null, "(Root)", ""));
        SelectedParentAccount = ParentAccounts.FirstOrDefault();
        RecomputeSuggestedCode();
    }

    /// <summary>
    /// يجب استدعاؤها قبل فتح الديالوج لملء ParentAccounts والعملات من DB
    /// </summary>
    public async Task InitializeAsync(Guid companyId)
    {
        _companyId = companyId;

        // ✅ جلب الحسابات والعملات معاً
        // ✅ جلب متسلسل (Sequential) لتجنب تعارض DbContext
        var rows = await _query.GetAccountsAsync(companyId);
        var currencyList = await _currencyLookup.GetActiveCurrenciesAsync();

        // --- ParentAccounts ---
        _flat = rows.ToList();

        var parents = rows
            .Where(a => !a.IsPosting)
            .OrderBy(a => a.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ParentAccounts.Clear();
        ParentAccounts.Add(new ParentAccountOptionVm(null, "(Root)", ""));

        foreach (var p in parents)
        {
            var indent = GetIndentFromCode(p.Code);
            var display = $"{indent}{p.Code} - {p.ArabicName}";
            ParentAccounts.Add(new ParentAccountOptionVm(p.Id, display, p.Code));
        }

        SelectedParentAccount = ParentAccounts.FirstOrDefault();
        RecomputeSuggestedCode();

        // ✅ --- Currencies من DB ---
        Currencies.Clear();
        foreach (var c in currencyList)
            Currencies.Add(c);

        // اختر USD افتراضياً إن وُجدت، وإلا أول عملة
        SelectedCurrency = Currencies.FirstOrDefault(c => c.Code == "USD")
                        ?? Currencies.FirstOrDefault();
    }

    private static string GetIndentFromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "";
        var depth = code.Count(c => c == '.');
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

    // ✅ نوع CurrencyOptionDto بدل string
    private CurrencyOptionDto? _selectedCurrency;
    public CurrencyOptionDto? SelectedCurrency
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
            ArabicNameError = "اسم الحساب بالعربية إلزامي.";
        }
        else
        {
            IsArabicNameValid = true;
            ArabicNameError = "";
        }
    }

    // ✅ BuildDraft يأخذ Code من CurrencyOptionDto
    public NewAccountDraftVm BuildDraft()
    {
        return new NewAccountDraftVm(
            CompanyId: _companyId,
            ParentId: SelectedParentAccount?.Id,
            Code: SuggestedCode,
            ArabicName: ArabicName.Trim(),
            EnglishName: string.IsNullOrWhiteSpace(EnglishName) ? null : EnglishName.Trim(),
            IsPosting: SelectedAccountType == "Postable",
            CurrencyCode: SelectedCurrency?.Code,
            SystemRole: string.IsNullOrWhiteSpace(SelectedSystemRole) ? null : SelectedSystemRole,
            Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            IsActive: IsActive
        );
    }

    private void RecomputeSuggestedCode()
    {
        var parentCode = SelectedParentAccount?.Code ?? "";

        if (string.IsNullOrWhiteSpace(parentCode))
        {
            // Root level: 1, 2, 3, ...
            var rootCodes = _flat
                .Where(a => !a.Code.Contains('.'))
                .Select(a => a.Code)
                .ToList();

            var next = ComputeNextSegment(rootCodes, "");
            SuggestedCode = next.ToString();
        }
        else
        {
            var children = _flat
                .Where(a => a.Code.StartsWith(parentCode + ".", StringComparison.Ordinal)
                         && a.Code.Count(c => c == '.') == parentCode.Count(c => c == '.') + 1)
                .Select(a => a.Code)
                .ToList();

            var next = ComputeNextSegment(children, parentCode);
            SuggestedCode = $"{parentCode}.{next}";
        }
    }

    private static int ComputeNextSegment(List<string> siblingCodes, string parentCode)
    {
        var maxSegment = 0;

        foreach (var code in siblingCodes)
        {
            var lastPart = string.IsNullOrWhiteSpace(parentCode)
                ? code
                : code.Substring(parentCode.Length + 1);

            var dotIndex = lastPart.IndexOf('.');
            var segment = dotIndex >= 0 ? lastPart.Substring(0, dotIndex) : lastPart;

            if (int.TryParse(segment, out var num) && num > maxSegment)
                maxSegment = num;
        }

        return maxSegment + 1;
    }

    private void RecomputeDerivedCategory()
    {
        var parentCode = SelectedParentAccount?.Code ?? "";

        if (string.IsNullOrWhiteSpace(parentCode))
        {
            DerivedCategoryText = "—";
            return;
        }

        var root = parentCode.Split('.')[0];

        DerivedCategoryText = root switch
        {
            "1" => "موجودات",
            "2" => "خصوم",
            "3" => "حقوق الملكية",
            "4" => "إيرادات",
            "5" => "مصروفات",
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

// ✅ Helper VMs
public sealed class ParentAccountOptionVm
{
    public Guid? Id { get; }
    public string DisplayName { get; }
    public string Code { get; }

    public ParentAccountOptionVm(Guid? id, string displayName, string code)
    {
        Id = id;
        DisplayName = displayName;
        Code = code;
    }
}

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