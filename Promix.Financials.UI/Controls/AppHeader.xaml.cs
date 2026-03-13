using Microsoft.UI.Xaml.Controls;
using System;
using System.Globalization;

namespace Promix.Financials.UI.Controls;

public sealed partial class AppHeader : UserControl
{
    public string SettingsTooltip => "الإعدادات";
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
        // ✅ التاريخ: اسم اليوم والشهر عربي — اليوم والسنة أرقام إنكليزية
        var now = DateTime.Now;
        var dayName = now.ToString("dddd", new CultureInfo("ar-SA"));   // الاثنين
        var monthName = now.ToString("MMMM", new CultureInfo("ar-SA")); // يناير
        var day = now.Day.ToString(CultureInfo.InvariantCulture);       // 13  ← إنكليزي
        var year = now.Year.ToString(CultureInfo.InvariantCulture);     // 2026 ← إنكليزي

        DateText.Text = $"{dayName}، {day} {monthName} {year}";
        // النتيجة: "الجمعة، 13 مارس 2026"

        SearchBox.PlaceholderText = "بحث...";
    }
}