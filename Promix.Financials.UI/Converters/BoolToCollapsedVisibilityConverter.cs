using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Promix.Financials.UI.Converters;

public sealed class BoolToCollapsedVisibilityConverter : IValueConverter
{
    // true => Collapsed, false => Visible (مناسب لعرض الخطأ عند invalid)
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}