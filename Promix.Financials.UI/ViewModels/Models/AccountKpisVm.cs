using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Promix.Financials.UI.ViewModels.Accounts.Models;

public sealed class AccountKpisVm : INotifyPropertyChanged
{
    private string _totalAssetsText = "—";
    public string TotalAssetsText { get => _totalAssetsText; set { _totalAssetsText = value; OnPropertyChanged(); } }

    private string _totalLiabilitiesText = "—";
    public string TotalLiabilitiesText { get => _totalLiabilitiesText; set { _totalLiabilitiesText = value; OnPropertyChanged(); } }

    private string _netEquityText = "—";
    public string NetEquityText { get => _netEquityText; set { _netEquityText = value; OnPropertyChanged(); } }

    private string _activeAccountsText = "—";
    public string ActiveAccountsText { get => _activeAccountsText; set { _activeAccountsText = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}