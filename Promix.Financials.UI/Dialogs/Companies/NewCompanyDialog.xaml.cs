using Microsoft.UI.Xaml.Controls;
using Promix.Financials.UI.ViewModels.Companies;

namespace Promix.Financials.UI.Dialogs.Companies;

public sealed partial class NewCompanyDialog : ContentDialog
{
    public NewCompanyDialog(NewCompanyDialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        PrimaryButtonClick += (_, args) =>
        {
            if (!vm.CanSubmit)
            {
                args.Cancel = true;
                vm.Validate();
            }
        };
    }
}