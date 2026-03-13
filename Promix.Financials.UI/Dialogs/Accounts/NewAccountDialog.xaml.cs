using Microsoft.UI.Xaml.Controls;
using Promix.Financials.UI.ViewModels.Accounts;

namespace Promix.Financials.UI.Dialogs.Accounts;

public sealed partial class NewAccountDialog : ContentDialog
{
    public NewAccountDialog(NewAccountDialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        PrimaryButtonClick += (_, args) =>
        {
            if (!vm.CanSubmit)
            {
                args.Cancel = true; // يمنع إغلاق الديالوج
                vm.Validate();
            }
        };
    }
    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (DataContext is Promix.Financials.UI.ViewModels.Accounts.NewAccountDialogViewModel vm)
        {
            vm.Validate();
            if (!vm.CanSubmit)
            {
                args.Cancel = true; // يمنع غلق الديالوج
            }
        }
    }
}