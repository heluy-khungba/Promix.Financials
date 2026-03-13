using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Runtime.Versioning;

namespace Promix.Financials.UI.Converters;

[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class EmptyToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => string.IsNullOrEmpty(value?.ToString())
            ? Visibility.Collapsed
            : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class AccountIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var type = value?.ToString();

        return type switch
        {
            "Group" => "\uE8B7",
            "Postable" => "\uE8C8",
            _ => "\uE946"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class AccountFontWeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var type = value?.ToString();

        return type == "Group"
            ? Microsoft.UI.Text.FontWeights.SemiBold
            : Microsoft.UI.Text.FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

[SupportedOSPlatform("windows10.0.19041.0")]
public sealed class AccountNameBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var type = value?.ToString();

        return type switch
        {
            "Group" => new SolidColorBrush(ColorHelper.FromArgb(255, 15, 23, 42)),
            "Postable" => new SolidColorBrush(ColorHelper.FromArgb(255, 71, 85, 105)),
            _ => new SolidColorBrush(Colors.Black)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}