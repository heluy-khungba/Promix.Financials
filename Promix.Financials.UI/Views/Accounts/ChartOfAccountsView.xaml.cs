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
using System.Threading.Tasks;

namespace Promix.Financials.UI.Views.Accounts;

public sealed partial class ChartOfAccountsView : Page
{
    private readonly ChartOfAccountsViewModel _vm;
    private readonly IServiceScope _scope; // ✅ hold scope alive for VM lifetime

    public ChartOfAccountsView()
    {
        InitializeComponent();

        var app = (App)Microsoft.UI.Xaml.Application.Current;

        // ✅ Resolve VM from a scope — prevents captive dependency crash
        _scope = app.Services.CreateScope();
        _vm = _scope.ServiceProvider.GetRequiredService<ChartOfAccountsViewModel>();

        DataContext = _vm;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded; // ✅ dispose scope when page is unloaded
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[OnLoaded] START");

            var userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();
            System.Diagnostics.Debug.WriteLine("[OnLoaded] userContext OK");

            var companyId = userContext.CompanyId ?? Guid.Empty;
            System.Diagnostics.Debug.WriteLine($"[OnLoaded] companyId = {companyId}");

            if (companyId == Guid.Empty)
            {
                System.Diagnostics.Debug.WriteLine("[OnLoaded] companyId is EMPTY — return");
                return;
            }

            System.Diagnostics.Debug.WriteLine("[OnLoaded] calling InitializeAsync...");
            await _vm.InitializeAsync(companyId);
            System.Diagnostics.Debug.WriteLine("[OnLoaded] InitializeAsync DONE");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnLoaded] EXCEPTION: {ex}");
            await ShowErrorAsync("خطأ عند التحميل", ex.ToString());
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _scope.Dispose();
    }

    // ✅ زر "حساب جديد" بدون حساب أب مسبق
    private async void NewAccount_Click(object sender, RoutedEventArgs e)
        => await OpenNewAccountDialogAsync(preselectedParentCode: null);

    // ✅ كليك يمين → إضافة حساب ابن
    private async void AddChildAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.Tag is not AccountNodeVm parentNode) return;
        await OpenNewAccountDialogAsync(preselectedParentCode: parentNode.Code);
    }

    private async Task OpenNewAccountDialogAsync(string? preselectedParentCode)
    {
        var app = (App)Microsoft.UI.Xaml.Application.Current;
        var userContext = app.Services.GetRequiredService<IUserContext>();
        var companyId = userContext.CompanyId ?? Guid.Empty;
        if (companyId == Guid.Empty) return;

        using var scope = app.Services.CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<NewAccountDialogViewModel>();
        await vm.InitializeAsync(companyId);

        if (!string.IsNullOrWhiteSpace(preselectedParentCode))
        {
            foreach (var p in vm.ParentAccounts)
            {
                if (p.Code == preselectedParentCode)
                {
                    vm.SelectedParentAccount = p;
                    break;
                }
            }
        }

        var dialog = new NewAccountDialog(vm) { XamlRoot = this.XamlRoot };
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var draft = vm.BuildDraft();
        var nature = DeriveNature(draft.Code);

        try
        {
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
            await ShowErrorAsync("تعذّر إنشاء الحساب", ex.Message);
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("خطأ غير متوقع", ex.Message);
        }
    }

    private static AccountNature DeriveNature(string code)
    {
        var root = code?.Split('.')[0] ?? "";
        return root switch
        {
            "2" or "3" or "4" => AccountNature.Credit,
            _ => AccountNature.Debit
        };
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        var dlg = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "حسناً",
            XamlRoot = this.XamlRoot
        };
        await dlg.ShowAsync();
    }

    private void AccountDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.DataContext is not AccountNodeVm account) return;
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
        => SetExpandStateAll(AccountsTreeView, true);

    private void CollapseAll_Click(object sender, RoutedEventArgs e)
        => SetExpandStateAll(AccountsTreeView, false);

    private static void SetExpandStateAll(DependencyObject parent, bool isExpanded)
    {
        for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TreeViewItem tvi)
                tvi.IsExpanded = isExpanded;
            SetExpandStateAll(child, isExpanded);
        }
    }
}