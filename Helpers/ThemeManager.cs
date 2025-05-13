using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AutoMarket.Helpers
{
    public static class ThemeManagerHelper
    {
        public static void SetDarkTheme(bool isDark)
        {
            // Удаляем предыдущую тему
            var existingTheme = Application.Current.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (existingTheme != null)
                Application.Current.Resources.MergedDictionaries.Remove(existingTheme);

            // Создаем новую тему
            var newTheme = new BundledTheme
            {
                BaseTheme = isDark ? BaseTheme.Dark : BaseTheme.Light,
                PrimaryColor = PrimaryColor.DeepOrange,
                SecondaryColor = SecondaryColor.Lime
            };

            // Применяем
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }

        public static bool IsDarkTheme()
        {
            var currentTheme = Application.Current.Resources.MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            return currentTheme?.BaseTheme == BaseTheme.Dark;
        }
    }
}

