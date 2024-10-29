using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace PixiDocks.Avalonia.Converters;

public class ControlIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Control;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
