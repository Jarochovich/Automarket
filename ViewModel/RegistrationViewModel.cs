using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class RegistrationViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _login;
        private string _password;
        private string _confirmPassword;
        private string _phoneNumber;

        public PasswordBox FirstPassBox { get; set; }
        public PasswordBox SecondPassBox { get; set; }

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
        public ICommand ShowAuthCommand { get; }
        public Action CloseAction { get; set; } // делегат для закрытия окна

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand(param => OnRegister(), (parameter) => CanRegister());
            ShowAuthCommand = new RelayCommand(param => ShowLoginWindow());
        }


        private void ShowLoginWindow()
        {
            var registerWindow = new AutorizationView();
            registerWindow.Show();

            CloseAction?.Invoke(); // Закрытие текущего окна
        }

        // Проверка, может ли быть выполнена регистрация
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Login) &&
           !string.IsNullOrWhiteSpace(Password) &&
           Password == ConfirmPassword && 
           !string.IsNullOrEmpty(PhoneNumber) &&// Проверяем, что пароли совпадают
           !HasValidationErrors();  // Убедимся, что нет ошибок валидации
        }

        // Регистрация
       private void OnRegister()
        {
            if (HasValidationErrors())
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки перед регистрацией", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика для регистрации пользователя
            // Пример регистрации

            if (DataWorker.CreateUser(Login, Password, PhoneNumber))
            {
                MessageBox.Show("Вы успешно зарегистрированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поля после успешной регистрации
                ClearFields();
            }
            else
            {
                MessageBox.Show("Пользователь уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ClearFields()
        {
            Login = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            PhoneNumber = string.Empty;
            // Очищаем поля PasswordBox в View
            if (FirstPassBox != null)
            {
                FirstPassBox.Clear(); // Очистка первого пароля
            }
            if (SecondPassBox != null)
            {
                SecondPassBox.Clear(); // Очистка второго пароля
            }
        }

        // Проверка ошибок в данных
        private bool HasValidationErrors()
        {
            var propertiesToCheck = new[] { nameof(ShowLoginWindow), nameof(Password), nameof(ConfirmPassword), nameof(PhoneNumber) };
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
                    nameof(ShowLoginWindow) => string.IsNullOrWhiteSpace(Login)
                        ? "Логин обязателен" : null,

                    nameof(Password) => string.IsNullOrWhiteSpace(Password)
                        ? "Пароль обязателен" : null,

                    nameof(ConfirmPassword) => Password != ConfirmPassword
                        ? "Пароли не совпадают" : null,

                    nameof(PhoneNumber) => string.IsNullOrWhiteSpace(PhoneNumber)
                        ? "Телефон обязателен"
                        : !Regex.IsMatch(PhoneNumber, @"^\d{7}$")
                            ? "Номер телефона должен содержать 7 цифр"
                            : null,

                    _ => null
                };
            }
        }

        public string Error => null;

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
