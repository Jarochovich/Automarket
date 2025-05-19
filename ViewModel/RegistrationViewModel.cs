using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class RegistrationViewModel : BaseViewModel, INotifyPropertyChanged, IDataErrorInfo
    {
        private string _login;
        private string _password;
        private string _confirmPassword;
        private string _phoneNumber = string.Empty; // вместо "+375"

        private readonly HashSet<string> _touchedProperties = new(); // Добавлено поле для отслеживания изменённых свойств

        private bool _isInitialLoad = true;

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
                OnPropertyChanged(nameof(Login));
            }
        }

        // Минимальная и максимальная длина логина
        private const int MinLoginLength = 4;
        private const int MaxLoginLength = 20;


        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                // Убираем только нецифровые символы, сохраняя код страны
                var newValue = Regex.Replace(value, @"[^\d]", "");
                if (newValue.StartsWith("375"))
                    newValue = "+" + newValue;

                _phoneNumber = newValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRegister));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                // Добавляем в "тронутые" только после начальной загрузки
                if (!_isInitialLoad)
                    _touchedProperties.Add(nameof(Password));
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                _touchedProperties.Add(nameof(ConfirmPassword)); // Помечаем как "тронутое"
                OnPropertyChanged(nameof(ConfirmPassword));
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
            // Не разрешаем регистрацию во время начальной загрузки
            if (_isInitialLoad) return false;

            return !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   IsValidPhoneNumber(PhoneNumber);
        }

        private bool IsValidPhoneNumber(string phone)
        {

            if (string.IsNullOrWhiteSpace(phone)) return false;

            var cleanPhone = CleanPhoneNumber(phone);
            return cleanPhone.Length == 12 &&
                   Regex.IsMatch(cleanPhone, @"^375(29|25|44|33|17)\d{7}$");
        }

        // Регистрация
        private async void OnRegister()
        {
            if (HasValidationErrors())
            {
                ShowMessageToUser("Пожалуйста, исправьте ошибки перед регистрацией");
                return;
            }

            // Добавляем await перед вызовом асинхронного метода
            bool isCreated = await DataWorker.CreateUserAsync(Login, Password, PhoneNumber);

            if (isCreated)
            {
                ShowMessageToUser("Регистрация прошла успешно");
                ClearFields();
                ResetValidation();
                ShowLoginWindow();
            }
            else
            {
                ShowMessageToUser("Пользователь уже существует");
            }
        }

        private void ClearFields()
        {
            Login = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            PhoneNumber = string.Empty; // Или string.Empty, в зависимости от ваших требований

            if (FirstPassBox != null) FirstPassBox.Clear();
            if (SecondPassBox != null) SecondPassBox.Clear();
        }

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
        public string this[string columnName]
        {
            get
            {
                // Пропускаем валидацию при первой загрузке
                if (_isInitialLoad)
                    return null;

                // Для паролей проверяем только если они не пустые
                if ((columnName == nameof(Password) || columnName == nameof(ConfirmPassword)))
                {
                    if (string.IsNullOrEmpty(Password) )return null;
                }

                return columnName switch
                {
                    nameof(Login) => ValidateLogin(),
                    nameof(Password) => ValidatePassword(),
                    nameof(ConfirmPassword) => Password != ConfirmPassword ? "Пароли не совпадают" : null,
                    nameof(PhoneNumber) => IsValidPhoneNumber(PhoneNumber)
                        ? null
                        : "Номер должен быть в формате 375 (XX) XXX-XX-XX",
                    _ => null
                };
            }
        }

        // Добавляем метод для завершения начальной загрузки
        public void CompleteInitialLoad()
        {
            _isInitialLoad = false;
        }

        private string ValidateLogin()
        {
            if (string.IsNullOrWhiteSpace(Login))
                return "Логин обязателен";

            if (Login.Length < MinLoginLength)
                return $"Логин должен содержать минимум {MinLoginLength} символа";

            if (Login.Length > MaxLoginLength)
                return $"Логин не должен превышать {MaxLoginLength} символов";

            // При необходимости можно добавить дополнительные проверки:
            if (!Regex.IsMatch(Login, @"^[a-zа-яА-ЯA-Z0-9]+$"))
                return "Логин может содержать только буквы и цифры";

            return null;
        }

        private string ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
                return "Пароль обязателен";

            var errors = new List<string>();

            if (Password.Length < 8)
                errors.Add("не менее 8 символов");

            if (!Password.Any(char.IsDigit))
                errors.Add("минимум 1 цифру");

            if (!Password.Any(char.IsUpper))
                errors.Add("минимум 1 заглавную букву");

            return errors.Count > 0
                ? $"Требования: {string.Join(", ", errors)}"
                : null;
        }

     

        // Очищаем номер телефона от всех символов, кроме цифр
        private string CleanPhoneNumber(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        // Добавим метод для сброса валидации
        public void ResetValidation()
        {
            _touchedProperties.Clear();
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(ConfirmPassword));
            OnPropertyChanged(nameof(PhoneNumber));
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
