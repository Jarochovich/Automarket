using AutoMarket.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class ProductDetailViewModel : BaseViewModel
    {
        public Product Product { get; }
        public ICommand AddToCartCommand { get; }

        private CartViewModel _cartViewModel;

        public ProductDetailViewModel(Product product, CartViewModel cartViewModel)
        {
            Product = product;
            _cartViewModel = cartViewModel;

            AddToCartCommand = new RelayCommand(_ => AddToCart());
        }

        private void AddToCart()
        {
            _cartViewModel?.AddToCart(Product);
        }
    }
}
