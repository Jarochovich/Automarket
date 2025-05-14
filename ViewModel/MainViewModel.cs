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
        private int _someValue;
        public int SomeValue
        {
            get => _someValue;
            set
            {
                if (_someValue != value)
                {
                    _someValue = value;
                    OnPropertyChanged(nameof(SomeValue));
                }
            }
        }

        private int _someNumber;
        public int SomeNumber
        {
            get => _someNumber;
            set => SetProperty(ref _someNumber, value);
        }

        private bool _isFeatureEnabled;
        public bool IsFeatureEnabled
        {
            get => _isFeatureEnabled;
            set => SetProperty(ref _isFeatureEnabled, value);
        }

        public ICommand ConfirmValueCommand { get; }





        // корзина
        public CartViewModel CartVM { get; set; } = new CartViewModel();

        // языки
        public ICommand SetRussianCommand { get; }
        public ICommand SetEnglishCommand { get; }

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
            AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());
            Categories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
            Manufacturers = new ObservableCollection<Manufacturer>(DataWorker.GetAllManufacturers());

            Products = new ObservableCollection<Product>(); // старт — пусто

            CartCommand = new RelayCommand(OpenCart);
            LogoutCommand = new RelayCommand(_ => Logout());
            OpenProductDetailsCommand = new RelayCommand(p => OpenProductDetails((Product)p));
            ResetFilterCommand = new RelayCommand(_ => ResetFilters());
            AddToCartCommand = new RelayCommand(ExecuteAddToCart);
            ProfileCommand = new RelayCommand(OpenAccount);

            SetRussianCommand = new RelayCommand(_ => App.ChangeLanguage("ru"));
            SetEnglishCommand = new RelayCommand(_ => App.ChangeLanguage("en"));

            ConfirmValueCommand = new RelayCommand(_ => OnConfirmValue());
        }

        private void OnConfirmValue()
        {
            // обработка подтверждения
        }

        public void RefreshProducts()
        {
            // Обновляем все товары из базы данных
            AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());

            // Применяем фильтрацию, если уже выбраны фильтры
            FilterProducts();
        }

        private void LoadProducts()
        {
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

        private void FilterProducts()
        {
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

            Products = new ObservableCollection<Product>(filtered);
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedManufacturer = null;
            SelectedCategory = null;
            MinPrice = null;
            MaxPrice = null;
            Products = new ObservableCollection<Product>();
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
                // Если пользователь не авторизован, показываем окно входа
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
                accountView.Owner = Application.Current.MainWindow; // Устанавливаем владельца
                accountView.Show();

                // Не скрываем главное окно, а оставляем его открытым
                // Application.Current.MainWindow?.Hide();
            }
        }

        private bool? ShowLoginDialog()
        {
            var loginView = new AutorizationView
            {
                Owner = Application.Current.MainWindow // Устанавливаем владельца
            };
            return loginView.ShowDialog();
        }

        private void ShowLoginWindow()
        {
            var loginView = new AutorizationView();
            loginView.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}