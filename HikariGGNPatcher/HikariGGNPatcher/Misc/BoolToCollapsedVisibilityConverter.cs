using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HikariGGNPatcher.Misc
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToCollapsedVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? val = value as bool?;

            if (!val.HasValue)
                return Visibility.Collapsed;

            bool invert;
            if (!bool.TryParse(parameter?.ToString(), out invert))
                invert = false;

            if (invert)
                val = !val;

            return val.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility? val = value as Visibility?;
            bool? invert = parameter as bool?;

            if (!val.HasValue)
                return false;

            bool result = val == Visibility.Visible;
            if (invert == true)
                result = !result;

            return result;
        }

        #endregion
    }
}
