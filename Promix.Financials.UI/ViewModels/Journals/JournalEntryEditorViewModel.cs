using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;
using Promix.Financials.Application.Features.Journals.Commands;
using Promix.Financials.Domain.Enums;
using Promix.Financials.UI.ViewModels.Journals.Models;

namespace Promix.Financials.UI.ViewModels.Journals;

public sealed class JournalEntryEditorViewModel : INotifyPropertyChanged
{
    public ObservableCollection<JournalAccountOptionVm> AccountOptions { get; }
    public ObservableCollection<JournalEntryTypeOptionVm> TypeOptions { get; } = new();
    public ObservableCollection<JournalEntryLineEditorVm> Lines { get; } = new();

    private JournalEntryTypeOptionVm? _selectedType;
    private DateTimeOffset _entryDate = DateTimeOffset.Now;
    private string _referenceNo = string.Empty;
    private string _description = string.Empty;

    public JournalEntryEditorViewModel(IEnumerable<JournalAccountOptionVm> accountOptions)
    {
        AccountOptions = new ObservableCollection<JournalAccountOptionVm>(accountOptions);

        TypeOptions.Add(new JournalEntryTypeOptionVm(JournalEntryType.DailyJournal, "قيد يومية"));
        TypeOptions.Add(new JournalEntryTypeOptionVm(JournalEntryType.ReceiptVoucher, "سند قبض"));
        TypeOptions.Add(new JournalEntryTypeOptionVm(JournalEntryType.PaymentVoucher, "سند صرف"));
        TypeOptions.Add(new JournalEntryTypeOptionVm(JournalEntryType.Adjustment, "قيد تسوية"));

        _selectedType = TypeOptions[0];

        AddLine();
        AddLine();
    }

    public JournalEntryTypeOptionVm? SelectedType
    {
        get => _selectedType;
        set
        {
            if (_selectedType == value) return;
            _selectedType = value;
            OnPropertyChanged();
        }
    }

    public DateTimeOffset EntryDate
    {
        get => _entryDate;
        set
        {
            if (_entryDate == value) return;
            _entryDate = value;
            OnPropertyChanged();
        }
    }

    public string ReferenceNo
    {
        get => _referenceNo;
        set
        {
            if (_referenceNo == value) return;
            _referenceNo = value;
            OnPropertyChanged();
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
        }
    }

    public double TotalDebit => Lines.Sum(x => x.Debit);
    public double TotalCredit => Lines.Sum(x => x.Credit);
    public double Difference => Math.Round(TotalDebit - TotalCredit, 2);
    public string BalanceStateText => Math.Abs(Difference) < 0.009 ? "متوازن" : "غير متوازن";
    public Brush BalanceStateBrush => JournalActivityBarVm.FromHex(Math.Abs(Difference) < 0.009 ? "#16A34A" : "#DC2626");

    public void AddLine()
    {
        var line = new JournalEntryLineEditorVm();
        line.PropertyChanged += OnLineChanged;
        Lines.Add(line);
        NotifyTotals();
    }

    public void RemoveLine(JournalEntryLineEditorVm line)
    {
        if (Lines.Count <= 2)
            return;

        line.PropertyChanged -= OnLineChanged;
        Lines.Remove(line);
        NotifyTotals();
    }

    public bool TryBuildCommand(Guid companyId, bool postNow, out CreateJournalEntryCommand? command, out string error)
    {
        command = null;
        error = string.Empty;

        var usableLines = Lines.Where(x => !x.IsEmpty).ToList();
        if (usableLines.Count < 2)
        {
            error = "أضف سطرين على الأقل في السند.";
            return false;
        }

        foreach (var line in usableLines)
        {
            if (line.SelectedAccountId is null)
            {
                error = "كل سطر يجب أن يحتوي على حساب.";
                return false;
            }

            var hasDebit = line.Debit > 0;
            var hasCredit = line.Credit > 0;
            if (hasDebit == hasCredit)
            {
                error = "كل سطر يجب أن يحتوي مدين أو دائن فقط.";
                return false;
            }
        }

        if (TotalDebit <= 0 || TotalCredit <= 0)
        {
            error = "يجب أن يحتوي السند على قيم مدينة ودائنة.";
            return false;
        }

        if (Math.Abs(Difference) >= 0.009)
        {
            error = "السند غير متوازن. يجب أن يتساوى الإجمالي المدين مع الدائن.";
            return false;
        }

        command = new CreateJournalEntryCommand(
            CompanyId: companyId,
            EntryDate: DateOnly.FromDateTime(EntryDate.Date),
            Type: SelectedType?.Value ?? JournalEntryType.DailyJournal,
            ReferenceNo: ReferenceNo,
            Description: Description,
            PostNow: postNow,
            Lines: usableLines
                .Select(x => new CreateJournalEntryLineCommand(
                    x.SelectedAccountId!.Value,
                    Convert.ToDecimal(x.Debit),
                    Convert.ToDecimal(x.Credit),
                    x.Description))
                .ToList());

        return true;
    }

    private void OnLineChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(JournalEntryLineEditorVm.Debit)
            or nameof(JournalEntryLineEditorVm.Credit)
            or nameof(JournalEntryLineEditorVm.SelectedAccountId)
            or nameof(JournalEntryLineEditorVm.IsEmpty))
        {
            NotifyTotals();
        }
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(TotalDebit));
        OnPropertyChanged(nameof(TotalCredit));
        OnPropertyChanged(nameof(Difference));
        OnPropertyChanged(nameof(BalanceStateText));
        OnPropertyChanged(nameof(BalanceStateBrush));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
