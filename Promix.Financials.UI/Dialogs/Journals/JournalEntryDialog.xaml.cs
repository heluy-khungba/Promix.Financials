using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Promix.Financials.Application.Features.Journals.Commands;
using Promix.Financials.UI.ViewModels.Journals;
using Promix.Financials.UI.ViewModels.Journals.Models;

namespace Promix.Financials.UI.Dialogs.Journals;

public sealed partial class JournalEntryDialog : ContentDialog
{
    private readonly Guid _companyId;

    public JournalEntryEditorViewModel ViewModel { get; }
    public CreateJournalEntryCommand? ResultCommand { get; private set; }

    public JournalEntryDialog(Guid companyId, IReadOnlyList<JournalAccountOptionVm> accounts)
    {
        InitializeComponent();
        _companyId = companyId;
        ViewModel = new JournalEntryEditorViewModel(accounts);
        DataContext = ViewModel;

        PrimaryButtonClick += OnPrimaryButtonClick;
        SecondaryButtonClick += OnSecondaryButtonClick;
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
        => ViewModel.AddLine();

    private void RemoveLine_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not JournalEntryLineEditorVm line) return;
        ViewModel.RemoveLine(line);
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        => ValidateAndBuild(postNow: false, args);

    private void OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        => ValidateAndBuild(postNow: true, args);

    private void ValidateAndBuild(bool postNow, ContentDialogButtonClickEventArgs args)
    {
        ErrorBanner.Visibility = Visibility.Collapsed;
        ErrorText.Text = string.Empty;

        if (!ViewModel.TryBuildCommand(_companyId, postNow, out var command, out var error))
        {
            args.Cancel = true;
            ErrorText.Text = error;
            ErrorBanner.Visibility = Visibility.Visible;
            return;
        }

        ResultCommand = command;
    }
}
