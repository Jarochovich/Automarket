using AutoMarket.Model;
using System.ComponentModel;

namespace AutoMarket.ViewModel
{
    public class CartItemViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public Product Product { get; }

        private int _countItem;
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

        public decimal TotalPrice => Product?.Price * CountItem ?? 0;

        public CartItemViewModel(Product product, int count = 1)
        {
            Product = product;
            CountItem = count;
        }
    }
}
