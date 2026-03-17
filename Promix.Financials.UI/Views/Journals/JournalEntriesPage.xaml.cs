using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.UI.Dialogs.Journals;
using Promix.Financials.UI.ViewModels.Journals;

namespace Promix.Financials.UI.Views.Journals;

public sealed partial class JournalEntriesPage : Page
{
    private readonly IServiceScope _scope;
    private readonly JournalEntriesViewModel _vm;
    private readonly IUserContext _userContext;

    public JournalEntriesPage()
    {
        InitializeComponent();

        var app = (App)Microsoft.UI.Xaml.Application.Current;
        _scope = app.Services.CreateScope();
        _vm = _scope.ServiceProvider.GetRequiredService<JournalEntriesViewModel>();
        _userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();

        DataContext = _vm;

        _vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(_vm.ErrorMessage) or null)
            {
                ErrorBannerText.Text = _vm.ErrorMessage ?? string.Empty;
                ErrorBanner.Visibility = _vm.HasError ? Visibility.Visible : Visibility.Collapsed;
            }

            if (args.PropertyName is nameof(_vm.SuccessMessage) or null)
            {
                SuccessBannerText.Text = _vm.SuccessMessage ?? string.Empty;
                SuccessBanner.Visibility = _vm.HasSuccess ? Visibility.Visible : Visibility.Collapsed;
            }
        };

        Loaded += OnLoaded;
        Unloaded += (_, __) => _scope.Dispose();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_userContext.CompanyId is null)
            return;

        await _vm.InitializeAsync(_userContext.CompanyId.Value);
        UpdateEmptyState();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await _vm.RefreshAsync();
        UpdateEmptyState();
    }

    private async void NewEntry_Click(object sender, RoutedEventArgs e)
    {
        if (_userContext.CompanyId is null)
            return;

        var dialog = new JournalEntryDialog(_userContext.CompanyId.Value, _vm.AccountOptions.ToList())
        {
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None || dialog.ResultCommand is null)
            return;

        await _vm.CreateAsync(dialog.ResultCommand);
        UpdateEmptyState();
    }

    private async void PostSelected_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedEntry is null)
            return;

        var confirm = new ContentDialog
        {
            Title = "ترحيل السند",
            Content = $"هل تريد ترحيل السند {_vm.SelectedEntry.EntryNumber} الآن؟",
            PrimaryButtonText = "ترحيل",
            CloseButtonText = "إلغاء",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary)
            return;

        await _vm.PostSelectedAsync();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
        => EmptyState.Visibility = _vm.Entries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
}
