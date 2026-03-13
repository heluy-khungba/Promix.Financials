using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI; // ✅ مهم جدًا (يحتوي Color)

namespace Promix.Financials.UI.Converters;

public sealed class AccountTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var type = value?.ToString();

        return type switch
        {
            "Group" => new SolidColorBrush(Color.FromArgb(255, 99, 102, 241)),  // Indigo
            "Postable" => new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)),  // Emerald
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class BoolToStatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b && b)
            return new SolidColorBrush(Color.FromArgb(255, 34, 197, 94)); // Green

        return new SolidColorBrush(Color.FromArgb(255, 239, 68, 68)); // Red
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class BoolToStatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool b && b) ? "Active" : "Inactive";

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}