using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InformaticTextBook.Converters
{
    public class LineHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is int lineNumber &&
                values[1] is int currentLine)
            {
                return lineNumber == currentLine
                    ? new SolidColorBrush(Colors.Yellow)
                    : new SolidColorBrush(Colors.Transparent);
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}