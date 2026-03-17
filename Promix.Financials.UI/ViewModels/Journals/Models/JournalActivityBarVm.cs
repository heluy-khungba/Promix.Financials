using System;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Promix.Financials.UI.ViewModels.Journals.Models;

public sealed record JournalActivityBarVm(
    string Label,
    string ValueText,
    double BarHeight,
    Brush FillBrush
)
{
    public static Brush FromHex(string hex) => new SolidColorBrush(ColorFromHex(hex));

    private static Color ColorFromHex(string hex)
    {
        var raw = hex.TrimStart('#');
        if (raw.Length == 6)
            raw = "FF" + raw;

        return Color.FromArgb(
            Convert.ToByte(raw[..2], 16),
            Convert.ToByte(raw.Substring(2, 2), 16),
            Convert.ToByte(raw.Substring(4, 2), 16),
            Convert.ToByte(raw.Substring(6, 2), 16));
    }
}
