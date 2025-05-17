using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class AutorizationViewModel : BaseViewModel, INotifyPropertyChanged, IDataErrorInfo 
    {
        private string _login;
        private string _password;
        public PasswordBox PassBox { get; set; }
        private bool _isFormTouched;

        public string Login
        {
            get => _login;
            set
            {
                _login = value.Trim();
                _isFormTouched = true;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value.Trim();
                _isFormTouched = true;
                OnPropertyChanged();
            }
        }

        public ICommand AuthCommand { get; }
        public ICommand ShowRegisterCommand { get; }

        public Action CloseAction { get; set; }

        public AutorizationViewModel()
        {
            AuthCommand = new RelayCommand(param => OnLogin(), param => CanLogin());
            ShowRegisterCommand = new RelayCommand(param => OpenRegisterView());
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnLogin()
        {
            if (HasValidationErrors())
            {
                ShowMessageToUser("Пожалуйста, исправьте ошибки перед авторизацией");
                return;
            }

            // Проверка администратора
            bool isAdmin = DataWorker.IsAdmin(Login, Password);
            if (isAdmin)
            {
                var adminUser = DataWorker.GetUserByLogin(Login);
                UserSession.Login(adminUser);

                ShowMessageToUser("Администратор! Вы успешно авторизовались!");
                AdminView adminView = new AdminView();
                adminView.Show();
                CloseAction?.Invoke();
                return;
            }

            // Проверка обычного пользователя
            if (DataWorker.GetUser(Login, Password))
            {
                var user = DataWorker.GetUserByLogin(Login);
                UserSession.Login(user);

                ShowMessageToUser("Вы успешно авторизовались!");
                ClearFields();

                var mainView = new MainView();
                mainView.Show();
                CloseAction?.Invoke();
            }
            else
            {
                ShowMessageToUser("Неверный логин или пароль");
            }
        }

        private void OpenRegisterView()
        {
            var registerView = new RegistrationView();
            registerView.Show();
            CloseAction?.Invoke();
        }

        private void ClearFields()
        {
            Login = string.Empty;
            Password = string.Empty;
            PassBox?.Clear();
        }

        private bool HasValidationErrors()
        {
            var propertiesToCheck = new[] { nameof(Login), nameof(Password) };
            return propertiesToCheck.Any(property => !string.IsNullOrEmpty(this[property]));
        }

        private readonly HashSet<string> _touchedProperties = new();

        public string this[string columnName]
        {
            get
            {
                if (!_touchedProperties.Contains(columnName))
                    return null;

                return columnName switch
                {
                    nameof(Login) => string.IsNullOrWhiteSpace(Login) ? "Логин обязателен" : null,
                    nameof(Password) => string.IsNullOrWhiteSpace(Password) ? "Пароль обязателен" : null,
                    _ => null
                };
            }
        }

        public string Error => null;



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