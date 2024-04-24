using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace PixiDocks.Avalonia.Helpers;

public class ConditionToSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true or not bool and not null)
        {
            if (parameter is string stringVal)
            {
                bool isNum = double.TryParse(stringVal, out double param);
                return isNum ? new GridLength(param, GridUnitType.Pixel) : new GridLength(1, GridUnitType.Star);
            }
        }

        return new GridLength(0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}