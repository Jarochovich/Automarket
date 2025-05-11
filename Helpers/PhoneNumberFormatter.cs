using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AutoMarket.Helpers
{
    public class PhoneNumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string phone && !string.IsNullOrEmpty(phone))
            {
                // Убираем все нецифровые символы
                phone = new string(phone.Where(char.IsDigit).ToArray());

                // Форматируем номер телефона
                if (phone.Length == 12)
                {
                    return $"+{phone.Substring(0, 3)} ({phone.Substring(3, 2)}) {phone.Substring(5, 3)}-{phone.Substring(8, 2)}-{phone.Substring(10, 2)}";
                }

                // Если длина не 12, возвращаем без изменений
                return phone;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var phone = value as string;
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Оставляем только цифры
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}
