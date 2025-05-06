using AutoMarket.Model;
using AutoMarket.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoMarket.ViewModel
{
    public class CartViewModel
    {
        public ObservableCollection<CartItem> CartItems { get; set; } = new ObservableCollection<CartItem>();
        public ObservableCollection<Product> CartProducts { get; set; } = new ObservableCollection<Product>();

        public decimal TotalPrice => CartItems.Sum(item => item.TotalPrice);


        public void AddToCart(Product product)
        {
            if (product != null)
            {
                CartProducts.Add(product);
                ShowMessageToUser($"Добавлен в корзину: {product.Name}");
            }
        }

        

        public void RemoveFromCart(CartItem item)
        {
            CartItems.Remove(item);
        }



        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            messageView.ShowDialog();
        }
    }
}
