using AutoMarket.Helpers;
using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                }
            }
        }

        public ICommand ToggleThemeCommand { get; }



        // языки
        public ICommand SetRussianCommand { get; }
        public ICommand SetEnglishCommand { get; }

        public BaseViewModel()
        {
            // Инициализация темы (лучше делать асинхронно, если загрузка тяжелая)
            _isDarkTheme = ThemeManagerHelper.IsDarkTheme();
            ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());

            SetRussianCommand = new RelayCommand(_ => App.ChangeLanguage("ru"));
            SetEnglishCommand = new RelayCommand(_ => App.ChangeLanguage("en"));
        }

        private void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
        }

        public void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            messageView.ShowDialog();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

