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
        // Тема
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
                }
            }
        }

        public ICommand ToggleThemeCommand { get; }
        public BaseViewModel()
        {
            _isDarkTheme = ThemeManagerHelper.IsDarkTheme();
            ToggleThemeCommand = new RelayCommand(_ => IsDarkTheme = !IsDarkTheme);
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
