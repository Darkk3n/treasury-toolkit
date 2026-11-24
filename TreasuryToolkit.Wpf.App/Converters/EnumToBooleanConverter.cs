using System.Globalization;
using System.Windows.Data;

namespace TreasuryToolkit.Wpf.App.Converters
{
    /// <summary>
    /// Binds a group of RadioButtons to one enum property: each button is checked when the value equals its
    /// ConverterParameter, and checking it sets the property to that value.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && value.Equals(parameter);

        // Unchecking (another button was picked) leaves the property alone
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is true ? parameter : Binding.DoNothing;
    }
}
