using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Promix.Financials.UI.ViewModels.Accounts;

public sealed class EditAccountDialogViewModel : INotifyPropertyChanged
{
    private readonly IAccountRepository _repo;

    public EditAccountDialogViewModel(IAccountRepository repo)
        => _repo = repo;

    // ─── ReadOnly ───────────────────────────────────────────��─────
    public Guid AccountId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = "";
    public string TypeText { get; private set; } = "";
    public string CurrencyDisplay { get; private set; } = "";
    public bool IsSystemAccount { get; private set; }

    // ─── قابلة للتعديل ───────────────────────────────────────────
    private string _arabicName = "";
    public string ArabicName
    {
        get => _arabicName;
        set
        {
            if (_arabicName == value) return;
            _arabicName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ValidationError));
        }
    }

    private string _englishName = "";
    public string EnglishName
    {
        get => _englishName;
        set { if (_englishName == value) return; _englishName = value; OnPropertyChanged(); }
    }

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        set { if (_isActive == value) return; _isActive = value; OnPropertyChanged(); }
    }

    private string _notes = "";
    public string Notes
    {
        get => _notes;
        set { if (_notes == value) return; _notes = value; OnPropertyChanged(); }
    }

    // ─── Validation ───────────────────────────────────────────────
    public bool CanSave => !string.IsNullOrWhiteSpace(ArabicName);
    public string? ValidationError => CanSave ? null : "الاسم العربي مطلوب";

    // ─── INotifyPropertyChanged ────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ─── تحميل البيانات ───────────────────────────────────────────
    public async Task InitializeAsync(Guid accountId, Guid companyId)
    {
        AccountId = accountId;
        CompanyId = companyId;

        var a = await _repo.GetByIdAsync(accountId, companyId);
        if (a is null) return;

        Code = a.Code;
        TypeText = a.IsPosting ? "حركي" : "تجميعي";
        CurrencyDisplay = a.CurrencyCode ?? "—";
        IsSystemAccount = a.SystemRole is not null;

        // ✅ NameAr / NameEn — الأسماء الصحيحة في Domain Entity
        ArabicName = a.NameAr;
        EnglishName = a.NameEn ?? "";
        IsActive = a.IsActive;
        Notes = a.Notes ?? "";
    }

    // ─── بناء Command ─────────────────────────────────────────────
    public EditAccountCommand BuildCommand() => new(
        AccountId: AccountId,
        CompanyId: CompanyId,
        ArabicName: ArabicName.Trim(),
        EnglishName: string.IsNullOrWhiteSpace(EnglishName) ? null : EnglishName.Trim(),
        IsActive: IsActive,
        Notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
    );
}