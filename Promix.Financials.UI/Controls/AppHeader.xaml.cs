using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using System;

namespace Promix.Financials.UI.Controls;

public sealed partial class AppHeader : UserControl
{
    private static readonly ResourceManager _resourceManager = new ResourceManager();
    private static readonly ResourceMap _map = _resourceManager.MainResourceMap;

    public string SettingsTooltip => R("Header_Settings.ToolTip", "Settings");
    public event EventHandler? SettingsRequested;
    public AppHeader()
    {
        InitializeComponent();
        ApplyTexts();
    }
    private void Settings_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }
    private void ApplyTexts()
    {
        var dateFormat = R("Header_Date.TodayFormat", "dddd, MMMM d, yyyy");
        DateText.Text = DateTime.Now.ToString(dateFormat);

        SearchBox.PlaceholderText = R("Header_Search.PlaceholderText", "Search...");
    }

    private static string R(string key, string fallback)
    {
        try
        {
            // IMPORTANT: prefix with "Resources/"
            return _map.GetValue($"Resources/{key}").ValueAsString;
        }
        catch
        {
            return fallback;
        }
    }
}