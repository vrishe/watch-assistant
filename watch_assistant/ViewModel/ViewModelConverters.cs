using System;
using System.Windows.Data;

namespace watch_assistant.ViewModel.Converters
{
    class FormatStringConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            return parameter != null ? String.Format((string)parameter, val) : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            return parameter != null ? val.Replace((string)parameter, String.Empty) : val;
        }

        #endregion
    }

    class ReferenceToDomainStringConverter : IValueConverter
    {
        #region Implementation

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return String.Empty;
            return Model.Search.VideoInfoGraber.GetServerName(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Takes no sense");
        }

        #endregion
    }
}
