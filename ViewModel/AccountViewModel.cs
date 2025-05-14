using AutoMarket.Model;
using AutoMarket.Model.Data;
using AutoMarket.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class AccountViewModel : BaseViewModel
    {
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        private ObservableCollection<Product> _purchasedProducts;
        public ObservableCollection<Product> PurchasedProducts
        {
            get => _purchasedProducts;
            set
            {
                _purchasedProducts = value;
                OnPropertyChanged(nameof(PurchasedProducts));
            }
        }

        public ICommand BackToMainCommand { get; }

        public AccountViewModel(User user)
        {
            CurrentUser = user;
            PurchasedProducts = new ObservableCollection<Product>();
            BackToMainCommand = new RelayCommand(_ => BackToMain());

            // Загрузка данных о покупках
            LoadPurchasedProducts();
        }

        private void LoadPurchasedProducts()
        {
            try
            {
                if (CurrentUser?.Id == null)
                {
                    MessageBox.Show("Пользователь не авторизован");
                    return;
                }

                PurchasedProducts.Clear();

                var products = DataWorker.GetPurchasedProducts(CurrentUser.Id);

                if (products == null || !products.Any())
                {
                    MessageBox.Show("У вас пока нет покупок");
                    return;
                }

                foreach (var product in products)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PurchasedProducts.Add(product);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки покупок: {ex.Message}");
                Debug.WriteLine($"Полная ошибка: {ex}");
            }
        }

        private void BackToMain()
        {

            // Закрываем текущее окно
            foreach (Window window in Application.Current.Windows)
            {
                if (window is AccountView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}