using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Promix.Financials.Application.Features.Accounts.Queries;
using Promix.Financials.UI.ViewModels.Accounts.Models;

namespace Promix.Financials.UI.ViewModels.Accounts;

public sealed class ChartOfAccountsViewModel : INotifyPropertyChanged
{
    private readonly IChartOfAccountsQuery _query;

    public ObservableCollection<FiscalYearOptionVm> FiscalYears { get; } = new();
    public ObservableCollection<AccountNodeVm> AccountTree { get; } = new();
    public AccountKpisVm Kpis { get; } = new();

    private List<AccountNodeVm> _fullTree = new();
    private Guid? _companyId;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            RefreshCommand.RaiseCanExecuteChanged();
        }
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private FiscalYearOptionVm? _selectedFiscalYear;
    public FiscalYearOptionVm? SelectedFiscalYear
    {
        get => _selectedFiscalYear;
        set
        {
            if (_selectedFiscalYear == value) return;
            _selectedFiscalYear = value;
            OnPropertyChanged();
        }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            ApplySearch();
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public ICommand NewAccountCommand { get; }

    public ChartOfAccountsViewModel(IChartOfAccountsQuery query)
    {
        _query = query;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy && _companyId is not null);
        NewAccountCommand = new RelayCommand(() => { });
        UpdateKpis();
    }

    public async Task InitializeAsync(Guid companyId)
    {
        _companyId = companyId;
        FiscalYears.Clear();
        RefreshCommand.RaiseCanExecuteChanged();
        await LoadAsync(companyId);
    }

    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        if (_companyId is null) return;
        await LoadAsync(_companyId.Value);
    }

    private async Task LoadAsync(Guid companyId)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var flat = await _query.GetAccountsAsync(companyId);
            _fullTree = BuildTree(flat);

            if (string.IsNullOrWhiteSpace(SearchText))
                RebindTree(_fullTree);
            else
                RebindTree(FilterTree(_fullTree, SearchText.Trim()));

            UpdateKpis();
        }
        catch (Exception ex)
        {
            AccountTree.Clear();
            _fullTree = new List<AccountNodeVm>();
            UpdateKpis();
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static List<AccountNodeVm> BuildTree(IReadOnlyList<AccountFlatDto> flat)
    {
        var map = flat.ToDictionary(
            x => x.Id,
            x => new AccountNodeVm(
                x.Id, x.Code,
                string.IsNullOrWhiteSpace(x.ArabicName) ? "—" : x.ArabicName,
                x.IsPosting, x.IsSystem, x.IsActive, x.ParentId));

        foreach (var node in map.Values)
        {
            if (node.ParentId is Guid pid && map.TryGetValue(pid, out var parent))
                parent.Children.Add(node);
        }

        var roots = map.Values
            .Where(x => x.ParentId is null)
            .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        SortRecursively(roots);
        return roots;

        static void SortRecursively(IList<AccountNodeVm> nodes)
        {
            foreach (var n in nodes)
            {
                if (n.Children.Count > 0)
                {
                    var sorted = n.Children
                        .OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    n.Children.Clear();
                    foreach (var s in sorted) n.Children.Add(s);
                    SortRecursively(sorted);
                }
            }
        }
    }

    private void RebindTree(IEnumerable<AccountNodeVm> nodes)
    {
        AccountTree.Clear();
        foreach (var node in nodes)
            AccountTree.Add(node);
    }

    private void UpdateKpis()
    {
        Kpis.TotalAssetsText = "—";
        Kpis.TotalLiabilitiesText = "—";
        Kpis.NetEquityText = "—";
        Kpis.ActiveAccountsText = CountActive(AccountTree).ToString();
    }

    private static int CountActive(IEnumerable<AccountNodeVm> nodes)
    {
        var total = 0;
        foreach (var node in nodes)
        {
            if (node.IsActive) total++;
            if (node.Children.Any()) total += CountActive(node.Children);
        }
        return total;
    }

    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            RebindTree(_fullTree);
            UpdateKpis();
            return;
        }

        var filtered = FilterTree(_fullTree, SearchText.Trim());
        RebindTree(filtered);
        UpdateKpis();
    }

    // ✅ دالة واحدة فقط — بدون تكرار
    private static List<AccountNodeVm> FilterTree(IEnumerable<AccountNodeVm> source, string search)
    {
        var result = new List<AccountNodeVm>();

        foreach (var node in source)
        {
            var childrenMatch = FilterTree(node.Children, search);

            var selfMatch =
                node.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                node.ArabicName.Contains(search, StringComparison.OrdinalIgnoreCase);

            if (!selfMatch && childrenMatch.Count == 0) continue;

            var copy = new AccountNodeVm(
                node.Id, node.Code, node.ArabicName,
                node.IsPosting, node.IsSystem, node.IsActive, node.ParentId);

            // إذا تطابق العقدة نفسها: أظهر كل أبنائها
            var childrenToShow = selfMatch
                ? (IEnumerable<AccountNodeVm>)node.Children
                : childrenMatch;

            foreach (var child in childrenToShow)
                copy.Children.Add(child);

            result.Add(copy);
        }

        return result;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter) => await ExecuteAsync();

    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;
        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}