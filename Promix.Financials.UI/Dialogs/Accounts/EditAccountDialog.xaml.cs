using Microsoft.UI.Xaml.Controls;
using Promix.Financials.UI.ViewModels.Accounts;

namespace Promix.Financials.UI.Dialogs.Accounts;

public sealed partial class EditAccountDialog : ContentDialog
{
    private readonly EditAccountDialogViewModel _vm;

    public EditAccountDialog(EditAccountDialogViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        // ✅ ربط زر الحفظ بـ CanSave
        Loaded += (_, _) => UpdatePrimaryButton();
        vm.PropertyChanged += (_, _) => UpdatePrimaryButton();
    }

    private void UpdatePrimaryButton()
        => IsPrimaryButtonEnabled = _vm.CanSave;
}