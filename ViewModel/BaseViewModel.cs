using AutoMarket.Helpers;
using AutoMarket.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    ThemeManagerHelper.SetDarkTheme(_isDarkTheme);
                    //OnPropertyChanged(nameof(IsDarkTheme));

                    // Можно сохранить настройку темы, если нужно
                    // Properties.Settings.Default.IsDarkTheme = _isDarkTheme;
                    // Properties.Settings.Default.Save();
                }
            }
        }

        public ICommand ToggleThemeCommand { get; }

        public BaseViewModel()
        {
            // Инициализация темы (лучше делать асинхронно, если загрузка тяжелая)
            _isDarkTheme = ThemeManagerHelper.IsDarkTheme();
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

