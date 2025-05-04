using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class AutorizationViewModel : INotifyPropertyChanged, IDataErrorInfo
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
        public ICommand ShowRegisterCommand { get; }  // Команда для показа RegistrationView

        public Action CloseAction { get; set; }

        public AutorizationViewModel()
        {
            AuthCommand = new RelayCommand(param => OnLogin(), param => CanLogin());
            ShowRegisterCommand = new RelayCommand(param => OpenRegisterView());  // Инициализируем команду
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password);  // Кнопка доступна, если оба поля заполнены
        }

        // логика входа
        private void OnLogin()
        {
            if (HasValidationErrors())
            {
                MessageBox.Show("Пожалуйста, исправьте ошибки перед авторизацией", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isAdmin = DataWorker.IsAdmin(Login, Password);
            if (isAdmin)
            {
                // Переход к AdminView
                MessageBox.Show("Адмнинистратор! Вы успешно авторизовались!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                AdminView adminView = new AdminView();
                adminView.Show();
                CloseAction?.Invoke();  // Закрыть окно авторизации
                return;
            }

            if (DataWorker.GetUser(Login, Password))
            {
                MessageBox.Show("Пользователь! Вы успешно авторизовались!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
                var mainView = new MainView();
                mainView.Show();

                // Закрываем окно авторизации
                CloseAction?.Invoke();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegisterView()
        {
            // Создаём и показываем окно регистрации
            var registerView = new RegistrationView();
            registerView.Show();

            // Закрываем окно авторизации
            CloseAction?.Invoke();
        }

        private void ClearFields()
        {
            Login = string.Empty;
            Password = string.Empty;
            // Очищаем поля PasswordBox в View
            if (PassBox != null)
            {
                PassBox.Clear(); // Очистка пароля
            }
        }

        private bool HasValidationErrors()
        {
            var propertiesToCheck = new[] { nameof(Login), nameof(Password) };
            foreach (var property in propertiesToCheck)
            {
                if (!string.IsNullOrEmpty(this[property])) // Если есть ошибка, вернуть true
                    return true;
            }
            return false; // Если нет ошибок
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
