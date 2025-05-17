using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AutoMarket.Helpers;
using AutoMarket.Model;
using AutoMarket.View;
using MaterialDesignThemes.Wpf;

namespace AutoMarket.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        // Пагинация
        private const int ItemsPerPage = 9;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                UpdatePagedProducts();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoToNextPage));
            }
        }

        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => CurrentPage < TotalPages;

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        // корзина
        public CartViewModel CartVM { get; set; } = new CartViewModel();

        // языки
        public ICommand SetRussianCommand { get; }
        public ICommand SetEnglishCommand { get; }

        private decimal _maxAvailablePrice;
        public decimal MaxAvailablePrice
        {
            get => _maxAvailablePrice;
            set
            {
                _maxAvailablePrice = value;
                OnPropertyChanged(nameof(MaxAvailablePrice));
                FilterProducts();
            }
        }

        private decimal? _minPrice;
        public decimal? MinPrice
        {
            get => _minPrice;
            set
            {
                _minPrice = value;
                OnPropertyChanged(nameof(MinPrice));
                FilterProducts();
            }
        }

        private decimal? _maxPrice;
        public decimal? MaxPrice
        {
            get => _maxPrice;
            set
            {
                _maxPrice = value;
                OnPropertyChanged(nameof(MaxPrice));
                FilterProducts();
            }
        }

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
                // При изменении всех товаров обновляем максимальную цену
                if (_allProducts != null && _allProducts.Any())
                {
                    MaxAvailablePrice = _allProducts.Max(p => p.Price);
                    MinPrice = 0;
                    MaxPrice = MaxAvailablePrice;
                }
            }
        }

        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged(nameof(FilteredProducts));
                UpdatePagedProducts();
            }
        }

        private ObservableCollection<Product> _pagedProducts;
        public ObservableCollection<Product> PagedProducts
        {
            get => _pagedProducts;
            set
            {
                _pagedProducts = value;
                OnPropertyChanged(nameof(PagedProducts));
            }
        }

        public ObservableCollection<Manufacturer> Manufacturers { get; set; }

        private Manufacturer _selectedManufacturer;
        public Manufacturer SelectedManufacturer
        {
            get => _selectedManufacturer;
            set
            {
                _selectedManufacturer = value;
                OnPropertyChanged(nameof(SelectedManufacturer));
                FilterProducts();
            }
        }

        public ICommand CartCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand OpenProductDetailsCommand { get; }
        public ICommand ResetFilterCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand ProfileCommand { get; }

        public MainViewModel()
        {
            // Инициализация коллекций
            AllProducts = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
            Manufacturers = new ObservableCollection<Manufacturer>(DataWorker.GetAllManufacturers());
            FilteredProducts = new ObservableCollection<Product>();
            PagedProducts = new ObservableCollection<Product>();

            // Команды
            CartCommand = new RelayCommand(OpenCart);
            LogoutCommand = new RelayCommand(_ => Logout());
            OpenProductDetailsCommand = new RelayCommand(p => OpenProductDetails((Product)p));
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            AddToCartCommand = new RelayCommand(ExecuteAddToCart);
            ProfileCommand = new RelayCommand(OpenAccount);

            SetRussianCommand = new RelayCommand(_ => App.ChangeLanguage("ru"));
            SetEnglishCommand = new RelayCommand(_ => App.ChangeLanguage("en"));

            NextPageCommand = new RelayCommand(_ => GoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());

            // Загрузка данных
            LoadAllProductsWithRatings();
        }

        private void LoadAllProductsWithRatings()
        {
            var products = DataWorker.GetAllProducts();
            if (products != null)
            {
                foreach (var product in products)
                {
                    var reviews = DataWorker.GetReviewsByProductId(product.Id);
                    product.Rating = reviews != null && reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                }
                AllProducts = new ObservableCollection<Product>(products);
            }
        }

        private void GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
            }
        }

        private void GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
            }
        }

        private void UpdatePagedProducts()
        {
            if (FilteredProducts == null || !FilteredProducts.Any())
            {
                PagedProducts = new ObservableCollection<Product>();
                return;
            }

            var skip = (CurrentPage - 1) * ItemsPerPage;
            var pagedItems = FilteredProducts.Skip(skip).Take(ItemsPerPage).ToList();
            PagedProducts = new ObservableCollection<Product>(pagedItems);
        }

        private void CalculateTotalPages()
        {
            if (FilteredProducts == null || ItemsPerPage <= 0)
            {
                TotalPages = 1;
                return;
            }

            TotalPages = (int)Math.Ceiling((double)FilteredProducts.Count / ItemsPerPage);
            TotalPages = Math.Max(1, TotalPages); // Минимум 1 страница
        }

        public void RefreshProducts()
        {
            LoadAllProductsWithRatings();
            FilterProducts();
        }

        private void LoadProducts()
        {
            if (SelectedCategory != null)
            {
                var products = DataWorker.GetProductsByCategory(SelectedCategory.Id);
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        var reviews = DataWorker.GetReviewsByProductId(product.Id);
                        product.Rating = reviews != null && reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                    }

                    FilteredProducts = new ObservableCollection<Product>(products);
                    CurrentPage = 1;
                    CalculateTotalPages();
                }
            }
            else
            {
                // Если категория не выбрана, показываем все товары
                FilteredProducts = new ObservableCollection<Product>(AllProducts);
                CurrentPage = 1;
                CalculateTotalPages();
            }
        }

        private void FilterProducts()
        {
            if (AllProducts == null) return;

            var filtered = AllProducts.AsEnumerable();

            if (SelectedCategory != null)
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(p => p.Name != null &&
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedManufacturer != null)
            {
                filtered = filtered.Where(p => p.ManufacturerId == SelectedManufacturer.Id);
            }

            if (MinPrice != null)
            {
                filtered = filtered.Where(p => p.Price >= MinPrice.Value);
            }

            if (MaxPrice != null)
            {
                filtered = filtered.Where(p => p.Price <= MaxPrice.Value);
            }

            var result = filtered.ToList();
            foreach (var product in result)
            {
                var reviews = DataWorker.GetReviewsByProductId(product.Id);
                product.Rating = reviews != null && reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            }

            FilteredProducts = new ObservableCollection<Product>(result);
            CurrentPage = 1;
            CalculateTotalPages();
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedManufacturer = null;
            SelectedCategory = null;
            MinPrice = 0;
            MaxPrice = MaxAvailablePrice;
            FilteredProducts = new ObservableCollection<Product>(AllProducts);
            CurrentPage = 1;
            CalculateTotalPages();
        }

        private void OpenProductDetails(Product product)
        {
            var viewModel = new ProductDetailViewModel(product, CartVM);
            var view = new ProductDetailView(viewModel);
            view.ShowDialog();
        }

        private void ExecuteAddToCart(object parameter)
        {
            if (parameter is Product product)
            {
                CartVM.AddToCart(product);
            }
        }

        private void OpenCart(object parameter)
        {
            var view = new CartView(CartVM);
            view.ShowDialog();
        }

        private void Logout()
        {
            UserSession.Logout();

            var authView = new AutorizationView();
            authView.Show();

            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }

        private User GetCurrentUser()
        {
            if (!UserSession.IsLoggedIn)
            {
                var loginResult = ShowLoginDialog();

                if (loginResult == true)
                {
                    return UserSession.CurrentUser;
                }
                return null;
            }

            return UserSession.CurrentUser;
        }

        private void OpenAccount(object parameter)
        {
            var currentUser = GetCurrentUser();

            if (currentUser != null)
            {
                var accountView = new AccountView();
                accountView.DataContext = new AccountViewModel(currentUser);
                accountView.Owner = Application.Current.MainWindow;
                accountView.Show();
            }
        }

        private bool? ShowLoginDialog()
        {
            var loginView = new AutorizationView
            {
                Owner = Application.Current.MainWindow
            };
            return loginView.ShowDialog();
        }
    }
}