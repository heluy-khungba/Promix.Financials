using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts.Commands;   // ✅ EditAccountCommand
using Promix.Financials.Application.Features.Accounts.Services;   // ✅ EditAccountService, DeleteAccountService
using Promix.Financials.Application.Features.Accounts;            // ✅ CreateAccountCommand, CreateAccountService
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
    private readonly IServiceScope _scope;

    public ChartOfAccountsView()
    {
        InitializeComponent();
        var app = (App)Microsoft.UI.Xaml.Application.Current;
        _scope = app.Services.CreateScope();
        _vm = _scope.ServiceProvider.GetRequiredService<ChartOfAccountsViewModel>();
        DataContext = _vm;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();
            var companyId = userContext.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty) return;
            await _vm.InitializeAsync(companyId);
        }
        catch (Exception ex) { await ShowErrorAsync("خطأ عند التحميل", ex.Message); }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
        => _scope.Dispose();

    // ─── إنشاء حساب جديد ──────────────────────────────────────��──
    private async void NewAccount_Click(object sender, RoutedEventArgs e)
        => await OpenNewAccountDialogAsync(preselectedParentCode: null);

    private async void AddChildAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.Tag is not AccountNodeVm parentNode) return;
        await OpenNewAccountDialogAsync(preselectedParentCode: parentNode.Code);
    }

    private async Task OpenNewAccountDialogAsync(string? preselectedParentCode)
    {
        try
        {
            var userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();
            var companyId = userContext.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty) return;

            using var scope = ((App)Microsoft.UI.Xaml.Application.Current).Services.CreateScope();
            var vm = scope.ServiceProvider.GetRequiredService<NewAccountDialogViewModel>();
            await vm.InitializeAsync(companyId);

            if (!string.IsNullOrWhiteSpace(preselectedParentCode))
                foreach (var p in vm.ParentAccounts)
                    if (p.Code == preselectedParentCode) { vm.SelectedParentAccount = p; break; }

            var dialog = new NewAccountDialog(vm) { XamlRoot = this.XamlRoot };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            var draft = vm.BuildDraft();
            var nature = DeriveNature(draft.Code);
            var createService = scope.ServiceProvider.GetRequiredService<CreateAccountService>();

            await createService.CreateAsync(new CreateAccountCommand(
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
            ));
            await _vm.InitializeAsync(companyId);
        }
        catch (Promix.Financials.Domain.Exceptions.BusinessRuleException ex)
        { await ShowErrorAsync("تعذّر إنشاء الحساب", ex.Message); }
        catch (Exception ex)
        { await ShowErrorAsync("خطأ غير متوقع", ex.Message); }
    }

    // ─── تعديل حساب ──────────────────────────────────────────────
    private async void EditAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.Tag is not AccountNodeVm node) return;
        await OpenEditAccountDialogAsync(node);
    }

    private async Task OpenEditAccountDialogAsync(AccountNodeVm node)
    {
        try
        {
            var userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();
            var companyId = userContext.CompanyId ?? Guid.Empty;
            if (companyId == Guid.Empty) return;

            using var scope = ((App)Microsoft.UI.Xaml.Application.Current).Services.CreateScope();
            var vm = scope.ServiceProvider.GetRequiredService<EditAccountDialogViewModel>();
            await vm.InitializeAsync(node.Id, companyId);

            var dialog = new EditAccountDialog(vm) { XamlRoot = this.XamlRoot };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            var editService = scope.ServiceProvider.GetRequiredService<EditAccountService>();
            await editService.EditAsync(vm.BuildCommand());
            await _vm.InitializeAsync(companyId);
        }
        catch (Promix.Financials.Domain.Exceptions.BusinessRuleException ex)
        { await ShowErrorAsync("تعذّر تعديل الحساب", ex.Message); }
        catch (Exception ex)
        { await ShowErrorAsync("خطأ غير متوقع", ex.Message); }
    }

    // ─── حذف حساب ────────────────────────────────────────────────
    private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item) return;
        if (item.Tag is not AccountNodeVm node) return;

        var confirm = new ContentDialog
        {
            Title = "تأكيد الحذف",
            Content = $"هل تريد حذف الحساب؟\n\nالكود: {node.Code}\nالاسم: {node.ArabicName}\n\n⚠️ لا يمكن التراجع عن هذا الإجراء.",
            PrimaryButtonText = "حذف",
            CloseButtonText = "إلغاء",
            XamlRoot = this.XamlRoot,
            DefaultButton = ContentDialogButton.Close
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

        try
        {
            var userContext = _scope.ServiceProvider.GetRequiredService<IUserContext>();
            var companyId = userContext.CompanyId ?? Guid.Empty;

            using var scope = ((App)Microsoft.UI.Xaml.Application.Current).Services.CreateScope();
            var deleteService = scope.ServiceProvider.GetRequiredService<DeleteAccountService>();
            await deleteService.DeleteAsync(node.Id, companyId);
            await _vm.InitializeAsync(companyId);
        }
        catch (Promix.Financials.Domain.Exceptions.BusinessRuleException ex)
        { await ShowErrorAsync("تعذّر حذف الحساب", ex.Message); }
        catch (Exception ex)
        { await ShowErrorAsync("خطأ غير متوقع", ex.Message); }
    }

    // ─── مساعدات ──────────────────────────────────────────────────
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
        if (item.Tag is not AccountNodeVm account) return;
        _ = new ContentDialog
        {
            Title = "تفاصيل الحساب",
            Content = $"الكود: {account.Code}\nالاسم: {account.ArabicName}\nالنوع: {account.TypeText}",
            CloseButtonText = "إغلاق",
            XamlRoot = this.XamlRoot
        }.ShowAsync();
    }

    private void ExpandAll_Click(object sender, RoutedEventArgs e)
        => SetExpandStateAll(AccountsTreeView, true);

    private void CollapseAll_Click(object sender, RoutedEventArgs e)
        => SetExpandStateAll(AccountsTreeView, false);

    private static void SetExpandStateAll(DependencyObject parent, bool isExpanded)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TreeViewItem tvi) tvi.IsExpanded = isExpanded;
            SetExpandStateAll(child, isExpanded);
        }
    }
}