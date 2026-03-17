using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Promix.Financials.Application.Features.Journals.Commands;
using Promix.Financials.Application.Features.Journals.Queries;
using Promix.Financials.Application.Features.Journals.Services;
using Promix.Financials.Domain.Enums;
using Promix.Financials.UI.ViewModels.Journals.Models;

namespace Promix.Financials.UI.ViewModels.Journals;

public sealed class JournalEntriesViewModel : INotifyPropertyChanged
{
    private readonly IJournalEntriesQuery _query;
    private readonly CreateJournalEntryService _createService;
    private readonly PostJournalEntryService _postService;
    private Guid _companyId;

    public ObservableCollection<JournalEntryRowVm> Entries { get; } = new();
    public ObservableCollection<JournalAccountOptionVm> AccountOptions { get; } = new();
    public ObservableCollection<JournalActivityBarVm> ActivityBars { get; } = new();

    private JournalEntryRowVm? _selectedEntry;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;
    private DateTimeOffset? _lastRefreshedAt;

    public JournalEntriesViewModel(
        IJournalEntriesQuery query,
        CreateJournalEntryService createService,
        PostJournalEntryService postService)
    {
        _query = query;
        _createService = createService;
        _postService = postService;
    }

    public JournalEntryRowVm? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (_selectedEntry == value) return;
            _selectedEntry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanPostSelected));
            OnPropertyChanged(nameof(SelectedSummaryText));
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (_successMessage == value) return;
            _successMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSuccess));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool CanPostSelected => SelectedEntry is { IsDraft: true };

    public string TotalEntriesText => Entries.Count.ToString();
    public string PostedEntriesText => Entries.Count(x => x.Status == JournalEntryStatus.Posted).ToString();
    public string DraftEntriesText => Entries.Count(x => x.Status == JournalEntryStatus.Draft).ToString();
    public string MovementVolumeText => Entries.Sum(x => x.TotalDebit).ToString("N0");
    public string LastRefreshedText => _lastRefreshedAt is null ? "آخر تحديث: الآن" : $"آخر تحديث: {_lastRefreshedAt.Value:HH:mm}";
    public string SelectedSummaryText => SelectedEntry is null
        ? "اختر قيداً من القائمة لعرض حالته وترحيله إذا كان مسودة."
        : $"{SelectedEntry.TypeText} · {SelectedEntry.EntryNumber} · {SelectedEntry.TotalDebitText}";

    public async Task InitializeAsync(Guid companyId)
    {
        _companyId = companyId;
        await LoadAsync();
    }

    public async Task RefreshAsync()
    {
        if (_companyId == Guid.Empty)
            return;

        await LoadAsync();
    }

    public async Task<bool> CreateAsync(CreateJournalEntryCommand command)
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;
            await _createService.CreateAsync(command);
            SuccessMessage = command.PostNow
                ? "تم حفظ السند وترحيله بنجاح."
                : "تم حفظ السند كمسودة بنجاح.";
            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    public async Task<bool> PostSelectedAsync()
    {
        if (SelectedEntry is null || _companyId == Guid.Empty)
            return false;

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;
            await _postService.PostAsync(new PostJournalEntryCommand(_companyId, SelectedEntry.Id));
            SuccessMessage = $"تم ترحيل السند {SelectedEntry.EntryNumber}.";
            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return false;
        }
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var entries = await _query.GetEntriesAsync(_companyId);
            var accounts = await _query.GetPostingAccountsAsync(_companyId);

            Entries.Clear();
            foreach (var entry in entries)
            {
                Entries.Add(new JournalEntryRowVm(
                    entry.Id,
                    entry.EntryNumber,
                    entry.EntryDate,
                    (JournalEntryType)entry.Type,
                    (JournalEntryStatus)entry.Status,
                    entry.ReferenceNo,
                    entry.Description,
                    entry.TotalDebit,
                    entry.TotalCredit,
                    entry.LineCount));
            }

            AccountOptions.Clear();
            foreach (var account in accounts)
                AccountOptions.Add(new JournalAccountOptionVm(account.Id, account.Code, account.NameAr));

            RebuildActivityBars();
            _lastRefreshedAt = DateTimeOffset.Now;
            OnPropertyChanged(nameof(LastRefreshedText));
            OnPropertyChanged(nameof(TotalEntriesText));
            OnPropertyChanged(nameof(PostedEntriesText));
            OnPropertyChanged(nameof(DraftEntriesText));
            OnPropertyChanged(nameof(MovementVolumeText));
            OnPropertyChanged(nameof(SelectedSummaryText));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RebuildActivityBars()
    {
        ActivityBars.Clear();

        var start = DateOnly.FromDateTime(DateTime.Today.AddDays(-6));
        var grouped = Entries
            .GroupBy(x => x.EntryDateText)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(y => y.TotalDebit));

        var days = Enumerable.Range(0, 7)
            .Select(offset => start.AddDays(offset))
            .ToList();

        var max = days
            .Select(day =>
            {
                var key = day.ToString("yyyy-MM-dd");
                return grouped.TryGetValue(key, out var amount) ? amount : 0m;
            })
            .DefaultIfEmpty(0m)
            .Max();

        foreach (var day in days)
        {
            var key = day.ToString("yyyy-MM-dd");
            var amount = grouped.TryGetValue(key, out var value) ? value : 0m;
            var barHeight = max <= 0 ? 16 : 16 + (double)(amount / max) * 70;

            ActivityBars.Add(new JournalActivityBarVm(
                day.ToString("dd"),
                amount.ToString("N0"),
                Math.Round(barHeight, 1),
                JournalActivityBarVm.FromHex(amount > 0 ? "#38BDF8" : "#CBD5E1")));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
