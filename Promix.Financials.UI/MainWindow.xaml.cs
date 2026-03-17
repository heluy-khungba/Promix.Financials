using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Auth;
using Promix.Financials.UI.Controls;
using Promix.Financials.UI.Views;
using System;
using Promix.Financials.UI.Views.Accounts;
using Promix.Financials.UI.Views.Journals;
namespace Promix.Financials.UI;

public sealed partial class MainWindow : Window
{
    private readonly IUserContext _userContext;
    private readonly IAuthService _authService;
    private readonly IUserContextBootstrapper _bootstrapper;
    public MainWindow()
    {
        InitializeComponent();

        var services = ((App)Microsoft.UI.Xaml.Application.Current).Services;
        _userContext = services.GetRequiredService<IUserContext>();
        _authService = services.GetRequiredService<IAuthService>();
        _bootstrapper = services.GetRequiredService<IUserContextBootstrapper>();

        Header.SettingsRequested += (_, __) =>
        {
            RootFrame.Navigate(typeof(SettingsView));
        };

        InitializeNavigation();
    }

    private void InitializeNavigation()
    {
        if (!_userContext.IsAuthenticated)
        {
            Header.Visibility = Visibility.Collapsed;
            Sidebar.Visibility = Visibility.Collapsed;
            RootFrame.Navigate(typeof(LoginView));
            return;
        }

        // ✅ authenticated but company not selected yet
        if (_userContext.CompanyId is null)
        {
            Header.Visibility = Visibility.Collapsed;
            Sidebar.Visibility = Visibility.Collapsed;
            RootFrame.Navigate(typeof(CompanySelectionView));
            return;
        }

        Header.Visibility = Visibility.Visible;
        Sidebar.Visibility = Visibility.Visible;
        RootFrame.Navigate(typeof(DashboardView));
    }

    public void RefreshAfterLogin()
    {
        InitializeNavigation();
    }
    public void RefreshAfterCompanySelected()
    {
        InitializeNavigation();
    }

    private void Sidebar_NavigateRequested(object sender, SidebarNavigateEventArgs e)
    {
        switch (e.Destination)
        {
            case SidebarDestination.Dashboard:
                RootFrame.Navigate(typeof(DashboardView));
                break;

            case SidebarDestination.ChartOfAccounts:
                RootFrame.Navigate(typeof(Promix.Financials.UI.Views.Accounts.ChartOfAccountsView));
                break;

            case SidebarDestination.Journals:
                RootFrame.Navigate(typeof(JournalEntriesPage));
                break;

            case SidebarDestination.Items:
                RootFrame.Navigate(typeof(Promix.Financials.UI.Views.ItemsPage));
                break;

            case SidebarDestination.Reports:
                RootFrame.Navigate(typeof(Promix.Financials.UI.Views.ReportsPage));
                break;

            case SidebarDestination.Settings:
                RootFrame.Navigate(typeof(SettingsView));
                break;
            case SidebarDestination.Currencies:
                RootFrame.Navigate(typeof(Promix.Financials.UI.Views.Currencies.CompanyCurrenciesView));
                break;
        }
    }


    private async void Sidebar_LogoutRequested(object sender, EventArgs e)
    {
        await _authService.LogoutAsync();

        // ✅ أعِد مزامنة IUserContext (سيصبح غير مسجل)
        await _bootstrapper.InitializeAsync();

        Header.Visibility = Visibility.Collapsed;
        Sidebar.Visibility = Visibility.Collapsed;

        RootFrame.Navigate(typeof(LoginView));
    }
}
