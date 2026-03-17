using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Promix.Financials.UI.ViewModels.Journals.Models;

public sealed class JournalEntryLineEditorVm : INotifyPropertyChanged
{
    private Guid? _selectedAccountId;
    private string _description = string.Empty;
    private double _debit;
    private double _credit;

    public Guid? SelectedAccountId
    {
        get => _selectedAccountId;
        set
        {
            if (_selectedAccountId == value) return;
            _selectedAccountId = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (_description == value) return;
            _description = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public double Debit
    {
        get => _debit;
        set
        {
            if (Math.Abs(_debit - value) < 0.0001) return;
            _debit = value < 0 ? 0 : value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public double Credit
    {
        get => _credit;
        set
        {
            if (Math.Abs(_credit - value) < 0.0001) return;
            _credit = value < 0 ? 0 : value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public bool IsEmpty =>
        SelectedAccountId is null &&
        string.IsNullOrWhiteSpace(Description) &&
        Debit <= 0 &&
        Credit <= 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
