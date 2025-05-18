using AutoMarket.Model;
using AutoMarket.Model.Data;
using AutoMarket.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class CartViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainVM;

        public CartViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;


            // Подписываемся на изменения коллекции
            CartItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (CartItemViewModel item in e.NewItems)
                        item.PropertyChanged += CartItem_PropertyChanged;

                if (e.OldItems != null)
                    foreach (CartItemViewModel item in e.OldItems)
                        item.PropertyChanged -= CartItem_PropertyChanged;

                RecalculateTotal();
            };


            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
            RemoveCommand = new RelayCommand(RemoveFromCart);
            PayCommand = new RelayCommand(ExecutePay);
            CartItems.CollectionChanged += (s, e) => RecalculateTotal();

            // 🔥 Подписываемся на изменения CountItem у уже добавленных элементов
            
        }

        private void RemoveFromCart(object parameter)
        {
            if (parameter is CartItemViewModel item)
            {
                //item.PropertyChanged -= CartItem_PropertyChanged; // 🧼
                CartItems.Remove(item);
                RecalculateTotal();
                //_mainVM.UpdateProductQuantity(item.Product.Id, +item.CountItem);
            }
        }

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

        private int _countItem;

        public Product Product { get; }
        public int CountItem
        {
            get => _countItem;
            set
            {
                if (_countItem != value)
                {
                    _countItem = value;
                    OnPropertyChanged(nameof(CountItem));
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


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
                        Status = Purchase.PurchaseStatus.Pending
                    };

                    DataWorker.SavePendingPurchase(tempPurchase);

                    // Получаем новое количество после уменьшения на складе
                    int newQuantity = DataWorker.DecreaseProductQuantity(item.Product.Id, item.CountItem);

                    if (newQuantity >= 0)
                    {
                        item.Product.Quantity = newQuantity;  // Обновляем локальное количество товара
                                                              // Если Product реализует INotifyPropertyChanged, UI обновится автоматически
                    }
                }

                CartItems.Clear();
                RecalculateTotal();

                // Если нужно обновить весь список продуктов из БД, раскомментируйте:
                // _mainVM.ReloadProducts();

                ShowMessageToUser("Оплата прошла успешно!");
                CartUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                ShowMessageToUser($"Ошибка: {ex.Message}");
            }
        }



        public void AddToCart(Product product)
        {
            if (product == null)
            {
                ShowMessageToUser("Ошибка: товар не найден");
                return;
            }

            // Получаем актуальное количество товара из базы
            int availableInDb = DataWorker.GetProductQuantity(product.Id);

            // Получаем общее количество этого товара уже в корзине
            int inCart = CartItems.Where(i => i.Product.Id == product.Id).Sum(i => i.CountItem);

            // Доступное количество = в базе - уже в корзине
            int available = availableInDb - inCart;

            if (available <= 0)
            {
                ShowMessageToUser("Нельзя добавить больше товара, чем есть на складе");
                return;
            }

            var existingItem = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);

            if (existingItem != null)
            {
                // Проверяем, что после увеличения CountItem не превысит availableInDb
                if (existingItem.CountItem + 1 > availableInDb)
                {
                    ShowMessageToUser("Нельзя добавить больше товара, чем есть на складе");
                    return;
                }
                existingItem.CountItem++;
            }
            else
            {
                // Для нового товара проверяем, что 1 <= availableInDb
                if (1 > availableInDb)
                {
                    ShowMessageToUser("Нельзя добавить больше товара, чем есть на складе");
                    return;
                }
                var newItem = new CartItemViewModel(product) { CountItem = 1 };
                newItem.PropertyChanged += CartItem_PropertyChanged;
                CartItems.Add(newItem);
            }

            ShowMessageToUser($"Добавлено: {product.Name}");
            RecalculateTotal();
            CartUpdated?.Invoke();
        }

        // Вспомогательный метод: сколько всего этого товара уже в корзине
        public int GetTotalInCart(int productId)
        {
            return CartItems.Where(i => i.Product.Id == productId).Sum(i => i.CountItem);
        }

        public void RemoveFromCart(CartItemViewModel item)
        {
            //item.PropertyChanged -= CartItem_PropertyChanged; // 🧼
            CartItems.Remove(item);
            RecalculateTotal();
            //_mainVM.UpdateProductQuantity(item.Product.Id, +item.CountItem);
        }

        private void IncreaseQuantity(object parameter)
        {
            if (parameter is CartItemViewModel item)
            {
                int availableInDb = DataWorker.GetProductQuantity(item.Product.Id);
                int totalInCart = CartItems.Where(i => i.Product.Id == item.Product.Id).Sum(i => i.CountItem);

                if (totalInCart < availableInDb)
                {
                    item.CountItem++; // Вызовет PropertyChanged -> RecalculateTotal()
                }
                else
                {
                    ShowMessageToUser("Нельзя добавить больше товара, чем есть на складе");
                }
            }
        }

        private void DecreaseQuantity(object parameter)
        {
            if (parameter is CartItemViewModel item && item.CountItem > 1)
            {
                item.CountItem--; // Вызовет PropertyChanged -> RecalculateTotal()
            }
        }

        private void CartItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Реагируем только на изменения CountItem и TotalPrice
            if (e.PropertyName == nameof(CartItemViewModel.CountItem))
                RecalculateTotal();
        }

        public event Action CartUpdated;

        private void RecalculateTotal()
        {
            TotalPrice = CartItems.Sum(i => i.TotalPrice);
            OnPropertyChanged(nameof(TotalPrice)); // Явное уведомление для UI
        }
    }
}
