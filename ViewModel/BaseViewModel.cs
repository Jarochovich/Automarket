using AutoMarket.Helpers;
using AutoMarket.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

