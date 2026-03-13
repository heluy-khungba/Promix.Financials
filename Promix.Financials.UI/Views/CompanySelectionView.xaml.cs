using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Companies;
using Promix.Financials.UI.Dialogs.Companies;
using Promix.Financials.UI.ViewModels.Companies;
using System;
using System.Threading.Tasks;

namespace Promix.Financials.UI.Views;

public sealed partial class CompanySelectionView : Page
{
    private readonly IServiceProvider _services;

    public CompanySelectionView()
    {
        InitializeComponent();

        _services = ((App)Microsoft.UI.Xaml.Application.Current).Services;

        Loaded += async (_, __) => await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        try
        {
            ErrorText.Text = string.Empty;
            SelectButton.IsEnabled = false;

            using var scope = _services.CreateScope();

            var companySelection = scope.ServiceProvider.GetRequiredService<ICompanySelectionService>();
            var bootstrapper = scope.ServiceProvider.GetRequiredService<IUserContextBootstrapper>();

            var companies = await companySelection.GetMyCompaniesAsync();
            CompaniesList.ItemsSource = companies;

            if (companies.Count == 1)
            {
                await companySelection.SelectCompanyAsync(companies[0].Id);
                await bootstrapper.InitializeAsync();
                NavigateToDashboard();
            }
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
    }

    private void CompaniesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectButton.IsEnabled = CompaniesList.SelectedItem is CompanySummaryDto;
    }

    private void CompaniesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        CompaniesList.SelectedItem = e.ClickedItem;
    }

    private async void OnNewCompanyClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            ErrorText.Text = string.Empty;

            using var scope = _services.CreateScope();

            var createCompany = scope.ServiceProvider.GetRequiredService<CreateCompanyService>();
            var currencyLookup = scope.ServiceProvider.GetRequiredService<ICurrencyLookupService>();
            var companyAdminRepository = scope.ServiceProvider.GetRequiredService<ICompanyAdminRepository>();
            var companySelection = scope.ServiceProvider.GetRequiredService<ICompanySelectionService>();
            var bootstrapper = scope.ServiceProvider.GetRequiredService<IUserContextBootstrapper>();

            var vm = new NewCompanyDialogViewModel();

            var nextCode = await companyAdminRepository.GenerateNextCompanyCodeAsync();
            var currencies = await currencyLookup.GetActiveCurrenciesAsync();

            vm.SetGeneratedCode(nextCode);
            vm.SetCurrencies(currencies, "USD");

            var dialog = new NewCompanyDialog(vm)
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            var data = vm.Build();

            var created = await createCompany.CreateAsync(
                new CreateCompanyCommand(data.Code, data.Name, data.BaseCurrency));

            await companySelection.SelectCompanyAsync(created.CompanyId);
            await bootstrapper.InitializeAsync();

            NavigateToDashboard();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
    }

    private async void OnSelectClicked(object sender, RoutedEventArgs e)
    {
        if (CompaniesList.SelectedItem is not CompanySummaryDto selected)
            return;

        try
        {
            SelectButton.IsEnabled = false;

            using var scope = _services.CreateScope();

            var companySelection = scope.ServiceProvider.GetRequiredService<ICompanySelectionService>();
            var bootstrapper = scope.ServiceProvider.GetRequiredService<IUserContextBootstrapper>();

            await companySelection.SelectCompanyAsync(selected.Id);
            await bootstrapper.InitializeAsync();

            NavigateToDashboard();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
            SelectButton.IsEnabled = true;
        }
    }

    private void NavigateToDashboard()
    {
        var app = (App)Microsoft.UI.Xaml.Application.Current;
        if (app.CurrentWindow is MainWindow mainWindow)
            mainWindow.RefreshAfterCompanySelected();
    }
}