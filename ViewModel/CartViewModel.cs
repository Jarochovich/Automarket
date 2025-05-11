using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{

    public class CartViewModel : INotifyPropertyChanged
    {
        public ICommand RemoveCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ObservableCollection<Product> CartProducts { get; set; } = new ObservableCollection<Product>();
        public CartViewModel()
        {
            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
            RemoveCommand = new RelayCommand(RemoveFromCart);
            CartItems.CollectionChanged += (s, e) => RecalculateTotal();
        }

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

        // Количество
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

        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            messageView.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
