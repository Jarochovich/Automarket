using AutoMarket.Model;
using AutoMarket.Model.Data;
using AutoMarket.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class CartViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ICommand RemoveCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand PayCommand { get; }

        public ObservableCollection<CartItemViewModel> CartItems { get; set; } = new ObservableCollection<CartItemViewModel>();

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public CartViewModel()
        {
            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
            RemoveCommand = new RelayCommand(RemoveFromCart);
            PayCommand = new RelayCommand(ExecutePay);

            CartItems.CollectionChanged += (s, e) => RecalculateTotal();
        }

        private void ExecutePay(object parameter)
        {
            if (CartItems.Count == 0)
            {
                ShowMessageToUser("Корзина пуста");
                return;
            }

            var currentUser = UserSession.CurrentUser;
            if (currentUser == null)
            {
                ShowMessageToUser("Пользователь не авторизован");
                return;
            }

            if (currentUser.Balance < TotalPrice)
            {
                ShowMessageToUser($"Недостаточно средств. Ваш баланс: {currentUser.Balance} BYN");
                return;
            }

            try
            {
                currentUser.Balance -= TotalPrice;
                DataWorker.UpdateUserBalance(currentUser.Id, -TotalPrice);

                foreach (var item in CartItems)
                {
                    var tempPurchase = new Purchase
                    {
                        UserId = currentUser.Id,
                        ProductId = item.Product.Id,
                        Quantity = item.CountItem,
                        PriceAtPurchase = item.Product.Price,
                        PurchaseDate = DateTime.Now,
                        Status = Purchase.PurchaseStatus.Pending // Это критически важно
                    };

                    DataWorker.SavePendingPurchase(tempPurchase);
                }

                CartItems.Clear();
                ShowMessageToUser("Оплата прошла успешно! Подтвердите получение товаров в личном кабинете.");
            }
            catch (Exception ex)
            {
                ShowMessageToUser($"Ошибка: {ex.Message}");
            }
        }

        public void AddToCart(Product product)
        {
            if (product == null) return;

            var existing = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.CountItem++;
            }
            else
            {
                var newItem = new CartItemViewModel(product);
                newItem.PropertyChanged += CartItem_PropertyChanged;
                CartItems.Add(newItem);
            }

            RecalculateTotal();
            ShowMessageToUser($"Добавлен в корзину: {product.Name}");
        }

        private void RemoveFromCart(object parameter)
        {
            if (parameter is CartItemViewModel item)
            {
                CartItems.Remove(item);
                item.PropertyChanged -= CartItem_PropertyChanged;
                RecalculateTotal();
            }
        }

        private void IncreaseQuantity(object parameter)
        {
            if (parameter is CartItemViewModel item && item.CountItem < 99)
            {
                item.CountItem++;
            }
        }

        private void DecreaseQuantity(object parameter)
        {
            if (parameter is CartItemViewModel item && item.CountItem > 1)
            {
                item.CountItem--;
            }
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItemViewModel.CountItem))
                RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            TotalPrice = CartItems.Sum(i => i.TotalPrice);
        }
    }
}
