using Promix.Financials.Application.Features.Companies;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Promix.Financials.UI.ViewModels.Companies;

public sealed class NewCompanyDialogViewModel : INotifyPropertyChanged
{
    private string _generatedCode = "";
    public string GeneratedCode
    {
        get => _generatedCode;
        set { _generatedCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); }
    }

    private string _name = "";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); }
    }

    private string _selectedCurrencyCode = "USD";
    public string SelectedCurrencyCode
    {
        get => _selectedCurrencyCode;
        set { _selectedCurrencyCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); }
    }

    public ObservableCollection<CurrencyOptionDto> AvailableCurrencies { get; } = new();

    private string? _error;
    public string? Error
    {
        get => _error;
        private set { _error = value; OnPropertyChanged(); }
    }

    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(GeneratedCode) &&
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(SelectedCurrencyCode);

    public void SetGeneratedCode(string code)
    {
        GeneratedCode = code.Trim().ToUpperInvariant();
    }

    public void SetCurrencies(IEnumerable<CurrencyOptionDto> currencies, string? preferredCode = "USD")
    {
        AvailableCurrencies.Clear();

        foreach (var currency in currencies)
            AvailableCurrencies.Add(currency);

        var preferred = AvailableCurrencies.FirstOrDefault(x => x.Code == preferredCode)
                     ?? AvailableCurrencies.FirstOrDefault();

        if (preferred is not null)
            SelectedCurrencyCode = preferred.Code;
    }

    public void Validate()
    {
        if (!CanSubmit)
            Error = "الرجاء تعبئة جميع الحقول المطلوبة.";
        else
            Error = null;
    }

    public (string Code, string Name, string BaseCurrency) Build()
        => (GeneratedCode.Trim(), Name.Trim(), SelectedCurrencyCode.Trim().ToUpperInvariant());

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}