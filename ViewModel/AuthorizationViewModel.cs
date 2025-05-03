using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class AuthorizationViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _login;
        private string _password;
        private string _confirmPassword;
        private string _phoneNumber;

        private bool _isFormTouched;  // Флаг для отслеживания взаимодействия с полями

        public string Login
        {
            get => _login;
            set
            {
                _login = value.Trim();
                _isFormTouched = true;  // Отмечаем, что пользователь взаимодействовал с полем
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value.Trim();
                _isFormTouched = true;  // Отмечаем, что пользователь взаимодействовал с полем
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value.Trim();
                _isFormTouched = true;  // Отмечаем, что пользователь взаимодействовал с полем
                OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value.Trim();
                _isFormTouched = true;  // Отмечаем, что пользователь взаимодействовал с полем
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }

        public AuthorizationViewModel()
        {
            RegisterCommand = new RelayCommand(param => OnRegister(), (parameter) => CanRegister());
        }

        // Проверка, может ли быть выполнена регистрация
        private bool CanRegister()
        {
            return _isFormTouched && !HasValidationErrors();
        }

        // Регистрация
        private void OnRegister()
        {
            if (HasValidationErrors())
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки перед регистрацией", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика регистрации
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel("Регистрация выполнена успешно!")
            };
            SetCenterPositionAndOpen(messageView);
        }

        // Проверка ошибок в данных
        private bool HasValidationErrors()
        {
            var propertiesToCheck = new[] { nameof(Login), nameof(Password), nameof(ConfirmPassword), nameof(PhoneNumber) };
            foreach (var property in propertiesToCheck)
            {
                if (!string.IsNullOrEmpty(this[property])) // Если есть ошибка, вернуть true
                    return true;
            }
            return false; // Если нет ошибок
        }

        // Валидация данных
        private readonly HashSet<string> _touchedProperties = new();

        public string this[string columnName]
        {
            get
            {
                if (!_touchedProperties.Contains(columnName))
                    return null;

                return columnName switch
                {
                    nameof(Login) => string.IsNullOrWhiteSpace(Login)
                        ? "Логин обязателен" : null,

                    nameof(Password) => string.IsNullOrWhiteSpace(Password)
                        ? "Пароль обязателен" : null,

                    nameof(ConfirmPassword) => Password != ConfirmPassword
                        ? "Пароли не совпадают" : null,

                    nameof(PhoneNumber) => string.IsNullOrWhiteSpace(PhoneNumber)
                        ? "Телефон обязателен"
                        : !Regex.IsMatch(PhoneNumber, @"^\d{10}$")
                            ? "Номер телефона должен содержать 10 цифр"
                            : null,

                    _ => null
                };
            }
        }

        public string Error => null;

        private void SetCenterPositionAndOpen(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (name != null)
                _touchedProperties.Add(name);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            CommandManager.InvalidateRequerySuggested();
        }
    }

}
