using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts;
using Promix.Financials.Domain.Enums;
using Promix.Financials.UI.Dialogs.Accounts;
using Promix.Financials.UI.ViewModels.Accounts;
using Promix.Financials.UI.ViewModels.Accounts.Models;
using System;

namespace Promix.Financials.UI.Views.Accounts;

public sealed partial class ChartOfAccountsView : Page
{
    private readonly ChartOfAccountsViewModel _vm;

    public ChartOfAccountsView()
    {
        InitializeComponent();

        var app = (App)Microsoft.UI.Xaml.Application.Current;
        _vm = app.Services.GetRequiredService<ChartOfAccountsViewModel>();

        DataContext = _vm;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var app = (App)Microsoft.UI.Xaml.Application.Current;
        var userContext = app.Services.GetRequiredService<IUserContext>();

        var companyId = userContext.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            return;

        await _vm.InitializeAsync(companyId);
    }

    private async void NewAccount_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)Microsoft.UI.Xaml.Application.Current;

        var userContext = app.Services.GetRequiredService<IUserContext>();
        var companyId = userContext.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty)
            return;

        var vm = app.Services.GetRequiredService<NewAccountDialogViewModel>();
        await vm.InitializeAsync(companyId);

        var dialog = new NewAccountDialog(vm)
        {
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        var draft = vm.BuildDraft();
        var nature = DeriveNature(draft.Code);

        try
        {
            using var scope = app.Services.CreateScope();
            var createService = scope.ServiceProvider.GetRequiredService<CreateAccountService>();

            var command = new CreateAccountCommand(
                CompanyId: draft.CompanyId,
                ParentId: draft.ParentId,
                Code: draft.Code,
                ArabicName: draft.ArabicName,
                EnglishName: draft.EnglishName,
                IsPosting: draft.IsPosting,
                Nature: nature,
                CurrencyCode: draft.CurrencyCode,
                SystemRole: draft.SystemRole,
                IsActive: draft.IsActive,
                Notes: draft.Notes
            );

            await createService.CreateAsync(command);

            await _vm.InitializeAsync(companyId);
        }
        catch (Promix.Financials.Domain.Exceptions.BusinessRuleException ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "تعذّر إنشاء الحساب",
                Content = ex.Message,
                CloseButtonText = "حسناً",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "خطأ غير متوقع",
                Content = ex.Message,
                CloseButtonText = "حسناً",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    private static AccountNature DeriveNature(string? code)
    {
        var root = code?.Split('.').FirstOrDefault() ?? "";
        return root switch
        {
            "2" or "3" or "4" => AccountNature.Credit,
            _ => AccountNature.Debit
        };
    }

    private void AccountDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item)
            return;

        if (item.DataContext is not AccountNodeVm account)
            return;

        var dialog = new ContentDialog
        {
            Title = "تفاصيل الحساب",
            Content = $"الكود: {account.Code}\nالاسم: {account.ArabicName}\nالنوع: {account.TypeText}",
            CloseButtonText = "إغلاق",
            XamlRoot = this.XamlRoot
        };

        _ = dialog.ShowAsync();
    }

    private void ExpandAll_Click(object sender, RoutedEventArgs e)
    {
        SetExpandStateForAllTreeItems(AccountsTreeView, true);
    }

    private void CollapseAll_Click(object sender, RoutedEventArgs e)
    {
        SetExpandStateForAllTreeItems(AccountsTreeView, false);
    }

    private static void SetExpandStateForAllTreeItems(DependencyObject parent, bool isExpanded)
    {
        if (parent is TreeViewItem treeViewItem)
        {
            treeViewItem.IsExpanded = isExpanded;
        }

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            SetExpandStateForAllTreeItems(child, isExpanded);
        }
    }
}