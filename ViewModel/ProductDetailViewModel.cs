using AutoMarket.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class ProductDetailViewModel : BaseViewModel
    {
        public Product Product { get; }

        private ObservableCollection<Review> _reviews;
        public ObservableCollection<Review> Reviews
        {
            get => _reviews;
            set
            {
                _reviews = value;
                OnPropertyChanged(nameof(Reviews));
            }
        }

        public ICommand AddToCartCommand { get; }

        private CartViewModel _cartViewModel;

        public ProductDetailViewModel(Product product, CartViewModel cartViewModel)
        {
            Product = product;
            _cartViewModel = cartViewModel;

            Reviews = new ObservableCollection<Review>(DataWorker.GetReviewsByProductId(product.Id));
            AddToCartCommand = new RelayCommand(_ => AddToCart());

            LoadReviews();
        }

        public ProductDetailViewModel(Product product)
        {
            Product = product;
        }

        private void LoadReviews()
        {
            var reviews = DataWorker.GetReviewsByProductId(Product.Id);
            Reviews = new ObservableCollection<Review>(reviews);
        }

        private void AddToCart()
        {
            _cartViewModel?.AddToCart(Product);
        }
    }
}
