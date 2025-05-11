using AutoMarket.Model;
using System.ComponentModel;

namespace AutoMarket.ViewModel
{
    public class CartItemViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
