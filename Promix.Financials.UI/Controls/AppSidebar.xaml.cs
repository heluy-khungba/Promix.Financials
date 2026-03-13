using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Promix.Financials.UI.Controls;

public sealed partial class AppSidebar : UserControl
{
    public event EventHandler<SidebarNavigateEventArgs>? NavigateRequested;
    public event EventHandler? LogoutRequested;

    public AppSidebar()
    {
        InitializeComponent();
    }

    public void SetUserInfo(string username, string companyName)
    {
        UserNameText.Text = username;
        CompanyText.Text = companyName;
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
        => NavigateRequested?.Invoke(this, new SidebarNavigateEventArgs(SidebarDestination.Dashboard));

    private void Items_Click(object sender, RoutedEventArgs e)
        => NavigateRequested?.Invoke(this, new SidebarNavigateEventArgs(SidebarDestination.Items));

    private void Reports_Click(object sender, RoutedEventArgs e)
        => NavigateRequested?.Invoke(this, new SidebarNavigateEventArgs(SidebarDestination.Reports));

    private void Settings_Click(object sender, RoutedEventArgs e)
        => NavigateRequested?.Invoke(this, new SidebarNavigateEventArgs(SidebarDestination.Settings));

    private void Logout_Click(object sender, RoutedEventArgs e)
        => LogoutRequested?.Invoke(this, EventArgs.Empty);
    private void ChartOfAccounts_Click(object sender, RoutedEventArgs e)
    => NavigateRequested?.Invoke(this, new SidebarNavigateEventArgs(SidebarDestination.ChartOfAccounts));
}

public enum SidebarDestination
{
    Dashboard,
    ChartOfAccounts,
    Items,
    Reports,
    Settings
}

public sealed class SidebarNavigateEventArgs : EventArgs
{
    public SidebarDestination Destination { get; }

    public SidebarNavigateEventArgs(SidebarDestination destination) => Destination = destination;
}