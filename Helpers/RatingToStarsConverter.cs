using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoMarket.Helpers
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double rating)
            {
                var stars = new List<bool>();
                int fullStars = (int)Math.Round(rating);

                for (int i = 1; i <= 5; i++)
                {
                    stars.Add(i <= fullStars);
                }

                return stars;
            }
            return Enumerable.Repeat(false, 5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
