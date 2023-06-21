using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Autumn.WPF.Annotations;

namespace Budget.Converter;

[ValueConverter]
public sealed class AmountToColorConverter : IValueConverter {
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is decimal d) {
            if (d < 0) {
                return Colors.Firebrick;
            } else {
                return Colors.Green;
            }
        }
        return Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
