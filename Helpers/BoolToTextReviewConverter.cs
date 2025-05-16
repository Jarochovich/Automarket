using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AutoMarket.Helpers
{
    public class BoolToTextReviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasReviewed)
            {
                return hasReviewed ? "Отзыв оставлен" : "Оставить отзыв";
            }
            return "Оставить отзыв";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}