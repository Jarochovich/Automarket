using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoMarket.Model;
using AutoMarket.View;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace AutoMarket.ViewModel
{
    class DataManageVM : BaseViewModel
    {
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ExecuteSearch();
            }
        }

        private ICommand _searchCommand;
        public ICommand SearchCommand => _searchCommand ??= new RelayCommand(_ => ExecuteSearch());

        public ICommand LoadImageCommand { get; }
        public ICommand LogoutCommand { get; }

        public DataManageVM()
        {
            LoadImageCommand = new RelayCommand(param => LoadImage());
            LogoutCommand = new RelayCommand(_ => Logout());

            // Инициализация коллекций
            AllUsers = new ObservableCollection<User>(DataWorker.GetAllUsers());
            AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());
            AllCategories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
            AllManufacturers = new ObservableCollection<Manufacturer>(DataWorker.GetAllManufacturers());
            AllReviews = new ObservableCollection<Review>(DataWorker.GetAllReviews());
            AllPurchases = new ObservableCollection<Purchase>(DataWorker.GetAllPurchases());
        }

        private TabItem _selectedTabItem;
        public TabItem SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                OnPropertyChanged(nameof(SelectedTabItem));
                ExecuteSearch();
            }
        }

        private void ExecuteSearch()
        {
            if (SelectedTabItem == null) return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                UpdateCurrentTabData();
                return;
            }

            var searchTextLower = SearchText.ToLower();

            switch (SelectedTabItem.Name)
            {
                case "UsersTab":
                    AllUsers = new ObservableCollection<User>(
                        DataWorker.GetAllUsers()
                            .Where(u => (u.Login?.ToLower().Contains(searchTextLower) ?? false) ||
                                       (u.PhoneNumber?.ToLower().Contains(searchTextLower) ?? false))
                            .ToList());
                    break;

                case "ProductsTab":
                    AllProducts = new ObservableCollection<Product>(
                        DataWorker.GetAllProducts()
                            .Where(p => (p.Name?.ToLower().Contains(searchTextLower) ?? false) ||
                                       (p.Description?.ToLower().Contains(searchTextLower) ?? false) ||
                                       p.Price.ToString().Contains(SearchText))
                            .ToList());
                    break;

                case "CategoriesTab":
                    AllCategories = new ObservableCollection<Category>(
                        DataWorker.GetAllCategories()
                            .Where(c => c.Name?.ToLower().Contains(searchTextLower) ?? false)
                            .ToList());
                    break;

                case "ManufacturersTab":
                    AllManufacturers = new ObservableCollection<Manufacturer>(
                        DataWorker.GetAllManufacturers()
                            .Where(m => m.Name?.ToLower().Contains(searchTextLower) ?? false)
                            .ToList());
                    break;

                case "ReviewsTab":
                    AllReviews = new ObservableCollection<Review>(
                        DataWorker.GetAllReviews()
                            .Where(r => (r.AuthorName?.ToLower().Contains(searchTextLower) ?? false) ||
                                        (r.Comment?.ToLower().Contains(searchTextLower) ?? false) ||
                                        r.Rating.ToString().Contains(SearchText))
                            .ToList());
                    break;

                case "PurchasesTab":
                    AllPurchases = new ObservableCollection<Purchase>(
                        DataWorker.GetAllPurchases()
                            .Where(p => (p.Status.ToString()?.ToLower().Contains(searchTextLower) ?? false) ||
                                      p.PurchaseDate.ToString().Contains(SearchText) ||
                                      p.PriceAtPurchase.ToString().Contains(SearchText))
                            .ToList());
                    break;
            }
        }

        private void UpdateCurrentTabData()
        {
            if (SelectedTabItem == null) return;

            switch (SelectedTabItem.Name)
            {
                case "UsersTab":
                    AllUsers = new ObservableCollection<User>(DataWorker.GetAllUsers());
                    break;
                case "ProductsTab":
                    AllProducts = new ObservableCollection<Product>(DataWorker.GetAllProducts());
                    break;
                case "CategoriesTab":
                    AllCategories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
                    break;
                case "ManufacturersTab":
                    AllManufacturers = new ObservableCollection<Manufacturer>(DataWorker.GetAllManufacturers());
                    break;
                case "ReviewsTab":
                    AllReviews = new ObservableCollection<Review>(DataWorker.GetAllReviews());
                    break;
                case "PurchasesTab":
                    AllPurchases = new ObservableCollection<Purchase>(DataWorker.GetAllPurchases());
                    break;
            }
        }

        private void Logout()
        {
            var authView = new AutorizationView();
            authView.Show();

            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?
                .Close();
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private void LoadImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение товара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ImagePath = openFileDialog.FileName;
                    ImageData = File.ReadAllBytes(ImagePath);
                    OnPropertyChanged(nameof(ImagePreview));
                }
                catch (Exception ex)
                {
                    ShowMessageToUser($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private ObservableCollection<Category> _allCategories;
        public ObservableCollection<Category> AllCategories
        {
            get => _allCategories;
            set
            {
                _allCategories = value;
                OnPropertyChanged(nameof(AllCategories));
            }
        }

        private ObservableCollection<Manufacturer> _allManufacturers;
        public ObservableCollection<Manufacturer> AllManufacturers
        {
            get => _allManufacturers;
            set
            {
                _allManufacturers = value;
                OnPropertyChanged(nameof(AllManufacturers));
            }
        }

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

        private ObservableCollection<User> _allUsers;
        public ObservableCollection<User> AllUsers
        {
            get => _allUsers;
            set
            {
                _allUsers = value;
                OnPropertyChanged(nameof(AllUsers));
            }
        }

        private ObservableCollection<Review> _allReviews;
        public ObservableCollection<Review> AllReviews
        {
            get => _allReviews;
            set
            {
                _allReviews = value;
                OnPropertyChanged(nameof(AllReviews));
            }
        }

        private ObservableCollection<Purchase> _allPurchases;
        public ObservableCollection<Purchase> AllPurchases
        {
            get => _allPurchases;
            set
            {
                _allPurchases = value;
                OnPropertyChanged(nameof(AllPurchases));
            }
        }
        public static string CategoryName { get; set; }

        private Category _categoryProduct;
        public Category CategoryProduct
        {
            get => _categoryProduct;
            set { _categoryProduct = value; OnPropertyChanged(nameof(CategoryProduct)); }
        }

        private Manufacturer _manufacturerProduct;
        public Manufacturer ManufacturerProduct
        {
            get => _manufacturerProduct;
            set { _manufacturerProduct = value; OnPropertyChanged(nameof(ManufacturerProduct)); }
        }

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(nameof(ProductName)); }
        }

        private int _quantityProduct;
        public int QuantityProduct
        {
            get => _quantityProduct;
            set { _quantityProduct = value; OnPropertyChanged(nameof(QuantityProduct)); }
        }

        private decimal _priceProduct;
        public decimal PriceProduct
        {
            get => _priceProduct;
            set { _priceProduct = value; OnPropertyChanged(nameof(PriceProduct)); }
        }

        private string _descriptionProduct;
        public string DescriptionProduct
        {
            get => _descriptionProduct;
            set { _descriptionProduct = value; OnPropertyChanged(nameof(DescriptionProduct)); }
        }

        public static byte[] ImageD { get; set; }

        public static string UserLogin { get; set; }
        public static string UserPassword { get; set; }
        public static int UserPhoneNumber { get; set; }
        public static string ManufacturerName { get; set; }

        private byte[] _imageData;
        public byte[] ImageData
        {
            get => _imageData;
            set
            {
                _imageData = value;
                ImageD = value;
                OnPropertyChanged(nameof(ImageData));
            }
        }

        public static User SelectedUser { get; set; }
        public static Category SelectedCategory { get; set; }
        public static Review SelectedReview { get; set; }
        public static Purchase SelectedPurchase { get; set; }
        public static Manufacturer SelectedManufacturer { get; set; }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(nameof(SelectedProduct)); }
        }

        public BitmapImage ImagePreview
        {
            get
            {
                if (ImageData == null || ImageData.Length == 0)
                    return null;

                try
                {
                    var image = new BitmapImage();
                    using (var ms = new MemoryStream(ImageData))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                    }
                    image.Freeze();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }

        #region COMMANDS_TO_EDIT
        private RelayCommand editProduct;
        public RelayCommand EditProduct
        {
            get
            {
                return editProduct ?? new RelayCommand(obj =>
                {
                    Window window = obj as Window;
                    if (SelectedProduct == null)
                    {
                        ShowMessageToUser("Не выбран продукт");
                        return;
                    }

                    string resultStr = DataWorker.EditProduct(
                        SelectedProduct,
                        CategoryProduct,
                        ManufacturerProduct,
                        ProductName,
                        QuantityProduct,
                        PriceProduct.ToString(),
                        DescriptionProduct,
                        ImageData);

                    UpdateCurrentTabData();
                    ShowMessageToUser(resultStr);
                    window.Close();
                    SetNullValuesToProperties();
                });
            }
        }
        #endregion

        #region COMMANDS_TO_ADD
        private RelayCommand addNewProduct;
        public RelayCommand AddNewProduct
        {
            get
            {
                return addNewProduct ?? new RelayCommand(obj =>
                {
                    Window window = obj as Window;
                    string resultStr = DataWorker.CreateProduct(
                        CategoryProduct,
                        ManufacturerProduct,
                        ProductName,
                        QuantityProduct,
                        PriceProduct,
                        DescriptionProduct,
                        ImageData);

                    UpdateCurrentTabData();
                    ShowMessageToUser(resultStr);
                    SetNullValuesToProperties();
                    window.Close();
                });
            }
        }
        #endregion

        #region COMMANDS_TO_DELETE
        private RelayCommand deleteItem;
        public RelayCommand DeleteItem
        {
            get
            {
                return deleteItem ?? new RelayCommand(obj =>
                {
                    if (SelectedTabItem == null)
                    {
                        ShowMessageToUser("Ничего не выбрано");
                        return;
                    }

                    string resultStr = "Ничего не выбрано";

                    switch (SelectedTabItem.Name)
                    {
                        case "UsersTab" when SelectedUser != null:
                            resultStr = DataWorker.DeleteUser(SelectedUser);
                            break;
                        case "ProductsTab" when SelectedProduct != null:
                            resultStr = DataWorker.DeleteProduct(SelectedProduct);
                            break;
                        case "ReviewsTab" when SelectedReview != null:
                            resultStr = DataWorker.DeleteReview(SelectedReview);
                            break;
                        case "PurchasesTab" when SelectedPurchase != null:
                            resultStr = DataWorker.DeletePurchase(SelectedPurchase);
                            break;
                    }

                    UpdateCurrentTabData();
                    ShowMessageToUser(resultStr);
                    SetNullValuesToProperties();
                });
            }
        }
        #endregion

        #region COMMANDS_OPEN_WINDOWS
        private RelayCommand openAddNewProduct;
        public RelayCommand OpenAddNewProduct
        {
            get
            {
                return openAddNewProduct ?? new RelayCommand(obj =>
                {
                    OpenAddProductWindow();
                });
            }
        }

        private RelayCommand openEditItem;
        public RelayCommand OpenEditItem
        {
            get
            {
                return openEditItem ?? new RelayCommand(obj =>
                {
                    if (SelectedTabItem?.Name == "ProductsTab" && SelectedProduct != null)
                    {
                        OpenEditProductWindow(SelectedProduct);
                    }
                    SetNullValuesToProperties();
                });
            }
        }
        #endregion

        #region METHODS_OPEN_WINDOW
        private void OpenAddProductWindow()
        {
            AddNewProductView addProductWindow = new AddNewProductView();
            SetCenterPositionAndOpen(addProductWindow);
        }

        private void OpenEditProductWindow(Product product)
        {
            EditProductView editProductWindow = new EditProductView(product);
            SetCenterPositionAndOpen(editProductWindow);
        }

        private void SetCenterPositionAndOpen(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        #endregion

        #region UPDATE_VIEWS
        private void SetNullValuesToProperties()
        {
            CategoryName = null;
            CategoryProduct = null;
            ProductName = null;
            QuantityProduct = 0;
            PriceProduct = 0;
            DescriptionProduct = null;
            UserLogin = null;
            UserPassword = null;
            UserPhoneNumber = 0;
            ManufacturerProduct = null;
        }
        #endregion
    }
}