using AutoMarket.Model;
using System.ComponentModel;

namespace AutoMarket.ViewModel
{
    public class CartItemViewModel : BaseViewModel
    {
        private Product _product;
        private int _countItem;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public int CountItem
        {
            get => _countItem;
            set
            {
                if (value <= 0)
                {
                    return; // Нельзя установить отрицательное количество
                }

                if (Product != null && value > Product.Quantity)
                {
                    return; // Нельзя добавить больше, чем есть на складе
                }

                _countItem = value;
                OnPropertyChanged(nameof(CountItem));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => Product != null ? Product.Price * CountItem : 0;

        public CartItemViewModel(Product product)
        {
            Product = product;
            CountItem = 1; // Начальное количество при добавлении в корзину
        }
    }
}
