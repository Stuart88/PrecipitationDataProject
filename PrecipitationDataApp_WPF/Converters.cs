using System;
using System.Globalization;
using System.Windows.Data;

namespace PrecipitationDataApp_WPF
{
    public class DatetoStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return "";

                case DateTime d:
                    return d.ToString("dd MMM yyyy");

                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.Parse(value as string);
        }

        #endregion Methods
    }
}