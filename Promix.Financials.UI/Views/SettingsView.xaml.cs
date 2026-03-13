using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using System;
using Windows.Globalization;
using Windows.Storage;

namespace Promix.Financials.UI.Views;

public sealed partial class SettingsView : Page
{
    private const string LanguageSettingKey = "AppLanguage";

    private bool _isInitializing;

    public SettingsView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadLanguages();
    }

    private void LoadLanguages()
    {
        _isInitializing = true;

        var saved = ApplicationData.Current.LocalSettings.Values[LanguageSettingKey] as string;
        var current = !string.IsNullOrWhiteSpace(saved)
            ? saved
            : (string.IsNullOrWhiteSpace(ApplicationLanguages.PrimaryLanguageOverride)
                ? "en-US"
                : ApplicationLanguages.PrimaryLanguageOverride);

        LanguageCombo.Items.Clear();

        var loader = new ResourceLoader();

        var enItem = new ComboBoxItem
        {
            Content = loader.GetString("Lang_English"),
            Tag = "en-US"
        };

        var arItem = new ComboBoxItem
        {
            Content = loader.GetString("Lang_Arabic"),
            Tag = "ar-SA"
        };

        LanguageCombo.Items.Add(enItem);
        LanguageCombo.Items.Add(arItem);

        LanguageCombo.SelectedItem =
            string.Equals(current, "ar-SA", StringComparison.OrdinalIgnoreCase) ? arItem : enItem;

        _isInitializing = false;
    }

    private async void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;

        if (LanguageCombo.SelectedItem is not ComboBoxItem cbi) return;
        var selectedLang = cbi.Tag as string;
        if (string.IsNullOrWhiteSpace(selectedLang)) return;

        var settings = ApplicationData.Current.LocalSettings;
        var current = settings.Values[LanguageSettingKey] as string
            ?? (string.IsNullOrWhiteSpace(ApplicationLanguages.PrimaryLanguageOverride)
                ? "en-US"
                : ApplicationLanguages.PrimaryLanguageOverride);

        if (string.Equals(current, selectedLang, StringComparison.OrdinalIgnoreCase))
            return;

        settings.Values[LanguageSettingKey] = selectedLang;

        var loader = new ResourceLoader();

        var dlg = new ContentDialog
        {
            Title = loader.GetString("RestartRequired_Title"),
            Content = loader.GetString("RestartRequired_Message"),
            PrimaryButtonText = loader.GetString("RestartNow"),
            CloseButtonText = loader.GetString("RestartLater"),
            XamlRoot = this.XamlRoot
        };

        var result = await dlg.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            RestartApp();
        }
    }

    private static void RestartApp()
    {
        var exe = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(exe))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe)
            {
                UseShellExecute = true
            });

        Environment.Exit(0);
    }
}