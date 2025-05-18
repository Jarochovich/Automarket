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
        //Заголовок
       
        private bool _showPopular = true;

        public string Header
        {
            get
            {
                if (_showPopular)
                    return "Популярные товары";
                else
                    return $"Категория: {SelectedCategory?.Name ?? "Все товары"}";
            }
        }

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
        public CartViewModel CartVM { get; }

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


        private Product _selectedProduct;

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public int Quantity
        {
            get => SelectedProduct?.Quantity ?? 0;
            set
            {
                if (SelectedProduct != null && SelectedProduct.Quantity != value)
                {
                    SelectedProduct.Quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
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
                    _showPopular = false;
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                    OnPropertyChanged(nameof(Header)); // уведомляем, что Header изменился
                    OnPropertyChanged(nameof(Quantity)); // Важно!

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
        public ICommand ShowPopularProductsCommand { get; }

        public MainViewModel()
        {
            CartVM = new CartViewModel(this);
            CartVM.CartUpdated += OnCartUpdated;
            // Инициализация коллекций
            ShowPopularProductsCommand = new RelayCommand(_ => ShowPopularProducts());
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
            ProfileCommand = new RelayCommand(_ => OpenAccount());

            // Пагинация
            NextPageCommand = new RelayCommand(_ => GoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());



            // Загрузка данных
            LoadAllProductsWithRatings();
            _showPopular = true;
            LoadProducts();
        }

        private void OnCartUpdated()
        {
            // Загружаем актуальные данные из базы
            var allProductsFromDb = DataWorker.GetAllProducts();

            foreach (var product in AllProducts)
            {
                var productFromDb = allProductsFromDb.FirstOrDefault(p => p.Id == product.Id);
                if (productFromDb != null)
                {
                    product.Quantity = productFromDb.Quantity;
                }
            }

            OnPropertyChanged(nameof(AllProducts));
            OnPropertyChanged(nameof(PagedProducts));
            OnPropertyChanged(nameof(FilteredProducts));
        }

        private void ShowPopularProducts()
        {
            SelectedCategory = null;
            SelectedManufacturer = null;

            var allProducts = DataWorker.GetAllProducts();

            foreach (var product in allProducts)
            {
                var reviews = DataWorker.GetReviewsByProductId(product.Id);
                product.Rating = reviews != null && reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            }

            var topRatedProducts = allProducts
                .Where(p => p.Rating >= 4)
                .OrderByDescending(p => p.Rating)
                .ToList();

            FilteredProducts = new ObservableCollection<Product>(topRatedProducts);

            _showPopular = true;
            OnPropertyChanged(nameof(Header));

            CurrentPage = 1;
            TotalPages = 1;

            OnPropertyChanged(nameof(FilteredProducts));
            OnPropertyChanged(nameof(Header));
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
                    //Header = SelectedCategory?.Name;

                    CurrentPage = 1;
                    CalculateTotalPages();
                }
            }
            else
            {
                // Устанавливаем рейтинг для всех товаров
                foreach (var product in AllProducts)
                {
                    var reviews = DataWorker.GetReviewsByProductId(product.Id);
                    product.Rating = reviews != null && reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                }

                var topRatedProducts = AllProducts
                    .Where(p => p.Rating >= 4)
                    .OrderByDescending(p => p.Rating)
                    .ToList();

                FilteredProducts = new ObservableCollection<Product>(topRatedProducts);
                OnPropertyChanged(nameof(Header));
                CurrentPage = 1;
                TotalPages = 1;
            }
            
        }

        public void UpdateProductQuantity(int productId, int delta)
        {
            var product = AllProducts.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                product.Quantity += delta;
                OnPropertyChanged(nameof(AllProducts));
                OnPropertyChanged(nameof(PagedProducts));
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        private void FilterProducts()
        {
            _showPopular = false;
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
            OnPropertyChanged(nameof(Header));
        }

        private void ResetFilters()
        {
            _showPopular = false;
            SearchText = string.Empty;
            SelectedManufacturer = null;
            SelectedCategory = null;
            MinPrice = 0;
            MaxPrice = MaxAvailablePrice;
            FilteredProducts = new ObservableCollection<Product>(AllProducts);
            CurrentPage = 1;
           
            CalculateTotalPages();
            OnPropertyChanged(nameof(Header));
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
                UpdateProductQuantities(); // Явное обновление
                OnPropertyChanged(nameof(Quantity)); // Обновляем привязку
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

        private void OpenAccount()
        {
            var accountVM = new AccountViewModel(UserSession.CurrentUser);

            // Подписываемся на событие
            accountVM.ProductQuantityUpdated += (productId, delta) =>
            {
                UpdateProductQuantity(productId, delta);
            };

            var accountView = new AccountView { DataContext = accountVM };
            accountView.ShowDialog();
        }

        private bool? ShowLoginDialog()
        {
            var loginView = new AutorizationView
            {
                Owner = Application.Current.MainWindow
            };
            return loginView.ShowDialog();
        }

        private void UpdateProductQuantities()
        {
            // Получаем актуальные данные из базы
            var allProductsFromDb = DataWorker.GetAllProducts();

            foreach (var product in AllProducts)
            {
                var productFromDb = allProductsFromDb.FirstOrDefault(p => p.Id == product.Id);
                if (productFromDb == null) continue;

                // Обновляем количество на основе данных из базы
                product.Quantity = productFromDb.Quantity;
            }

            OnPropertyChanged(nameof(AllProducts));
            OnPropertyChanged(nameof(PagedProducts));
            OnPropertyChanged(nameof(FilteredProducts));
        }

        //public void UpdateProductQuantity(int productId, int delta)
        //{
        //    var product = AllProducts.FirstOrDefault(p => p.Id == productId);
        //    if (product != null)
        //    {
        //        product.Quantity += delta;
        //        OnPropertyChanged(nameof(AllProducts));
        //    }
        //}

        public void ReloadProducts()
        {
            AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());
            OnPropertyChanged(nameof(AllProducts));
            // Если используешь PagedProducts — обнови и его
            UpdatePagedProducts();
        }
    }
}