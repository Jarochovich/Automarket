using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using AutoMarket.Model;
using AutoMarket.View;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace AutoMarket.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        // корзина
        public CartViewModel CartVM { get; set; } = new CartViewModel();

        

        private BitmapImage _imagePreview;
        public BitmapImage ImagePreview
        {
            get => _imagePreview;
            set
            {
                _imagePreview = value;
                OnPropertyChanged(nameof(ImagePreview));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterProducts();
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                    LoadProducts();
                }
            }
        }

        public ObservableCollection<Category> Categories { get; }

        private ObservableCollection<Product> _allProducts;
        public ObservableCollection<Product> AllProducts
        {
            get => _allProducts;
            set
            {
                _allProducts = value;
                OnPropertyChanged(nameof(AllProducts));
            }
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }

        public ICommand ProfileCommand { get; }
        public ICommand CartCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenProductDetailsCommand { get; }

        public MainViewModel()
        {
            Categories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
            ProfileCommand = new RelayCommand(_ => MessageBox.Show("Профиль"));
            CartCommand = new RelayCommand(OpenCart);
            LogoutCommand = new RelayCommand(_ => Logout());
            AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());
            Products = new ObservableCollection<Product>();
            OpenProductDetailsCommand = new RelayCommand(p => OpenProductDetails((Product)p)); // Передаем продукт через команду
        }

        private void LoadProducts()
        {
            // Загружаем продукты для выбранной категории
            if (SelectedCategory != null)
            {
                Products.Clear();
                foreach (var product in DataWorker.GetProductsByCategory(SelectedCategory.Id))
                {
                    Products.Add(product);
                }
            }
        }

        private void OpenProductDetails(Product product)
        {
            var viewModel = new ProductDetailViewModel(product, CartVM);
            var view = new ProductDetailView(viewModel);
            view.ShowDialog();
        }


        private void OpenCart(object parameter)
        {
            var view = new CartView(CartVM);
            view.ShowDialog();
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Products = new ObservableCollection<Product>(AllProducts);
            }
            else
            {
                var filtered = AllProducts.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                Products = new ObservableCollection<Product>(filtered);
            }
        }

        private void Logout()
        {
            var authView = new AutorizationView();
            authView.Show();

            // Закрытие текущего окна, связанного с этим ViewModel
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }


        public ICollectionView GroupedProductsView { get; private set; }

      
    }
}
