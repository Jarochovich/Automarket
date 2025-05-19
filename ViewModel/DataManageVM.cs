using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using AutoMarket.Model;
using System.Windows;
using AutoMarket.View;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;

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
        }



        private TabItem _selectedTabItem;
        public TabItem SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                OnPropertyChanged(nameof(SearchText));

            }
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Если строка поиска пустая, показываем все данные
                switch (SelectedTabItem?.Name)
                {
                    case "UsersTab":
                        AllUsers = DataWorker.GetAllUsers();
                        break;
                    case "ProductsTab":
                        AllProducts = DataWorker.GetAllProducts();
                        break;
                    case "CategoriesTab":
                        AllCategories = DataWorker.GetAllCategories();
                        break;
                    case "ManufacturersTab":
                        AllManufacturers = DataWorker.GetAllManufacturers();
                        break;
                    case "ReviewsTab":
                        AllReviews = DataWorker.GetAllReviews();
                        break;
                    case "PurchasesTab":
                        AllPurchases = DataWorker.GetAllPurchases();
                        break;
                }
                return;
            }

            var searchTextLower = SearchText.ToLower();

            switch (SelectedTabItem?.Name)
            {
                case "UsersTab":
                    AllUsers = DataWorker.GetAllUsers()
                        .Where(u => (u.Login != null && u.Login.ToLower().Contains(searchTextLower)) ||
                                   (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(searchTextLower)))
                        .ToList();
                    break;

                case "ProductsTab":
                    AllProducts = DataWorker.GetAllProducts()
                        .Where(p => (p.Name != null && p.Name.ToLower().Contains(searchTextLower)) ||
                                  (p.Description != null && p.Description.ToLower().Contains(searchTextLower)) ||
                                  p.Price.ToString().Contains(SearchText))
                        .ToList();
                    break;

                case "CategoriesTab":
                    AllCategories = DataWorker.GetAllCategories()
                        .Where(c => c.Name != null && c.Name.ToLower().Contains(searchTextLower))
                        .ToList();
                    break;

                case "ManufacturersTab":
                    AllManufacturers = DataWorker.GetAllManufacturers()
                        .Where(m => m.Name != null && m.Name.ToLower().Contains(searchTextLower))
                        .ToList();
                    break;

                case "ReviewsTab":
                    AllReviews = DataWorker.GetAllReviews()
                        .Where(r => (r.AuthorName != null && r.AuthorName.ToLower().Contains(searchTextLower)) ||
                                    (r.Comment != null && r.Comment.ToLower().Contains(searchTextLower)) ||
                                    r.Rating.ToString().Contains(SearchText))
                        .ToList();
                    break;

                case "PurchasesTab":
                    AllPurchases = DataWorker.GetAllPurchases()
                        .Where(p => (p.Status != null && p.Status.ToString().Contains(searchTextLower)) ||
                                  p.PurchaseDate.ToString().Contains(SearchText) ||
                                  p.PriceAtPurchase.ToString().Contains(SearchText))
                        .ToList();
                    break;
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
                    OnPropertyChanged(nameof(ImagePreview)); // Важно уведомить об изменении
                }
                catch (Exception ex)
                {
                    ShowMessageToUser($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }


        // все категории
        private List<Category> allCategories = DataWorker.GetAllCategories();
        public List<Category> AllCategories
        {
            get { return allCategories; }
            set { 
                allCategories = value;
                OnPropertyChanged("AllCategories");
                }
        }

        // все производители
        private List<Manufacturer> allManufacturers = DataWorker.GetAllManufacturers();
        public List<Manufacturer> AllManufacturers
        {
            get { return allManufacturers; }
            set
            {
                allManufacturers = value;
                OnPropertyChanged("AllManufacturers");
            }
        }

        // все продукты
        private List<Product> allProducts = DataWorker.GetAllProducts();
        public List<Product> AllProducts
        {
            get { return allProducts; }
            set
            {
                allProducts = value;
                OnPropertyChanged("AllProducts");
            }
        }

        // все пользователи
        private List<User> allUsers = DataWorker.GetAllUsers();
        public List<User> AllUsers
        {
            get { return allUsers; }
            set
            {
                allUsers = value;
                OnPropertyChanged("AllUsers");
            }
        }


        // все отзывы
        private List<Review> allReviews = DataWorker.GetAllReviews();
        public List<Review> AllReviews
        {
            get { return allReviews; }
            set
            {
                allReviews = value;
                OnPropertyChanged("AllReviews");
            }
        }

        // все заказы
        private List<Purchase> allPurchases = DataWorker.GetAllPurchases();
        public List<Purchase> AllPurchases
        {
            get { return allPurchases; }
            set
            {
                allPurchases = value;
                OnPropertyChanged("AllPurchases");
            }
        }


        // категория
        public static string CategoryName { get; set; }

        // продукт
        private Category _categoryProduct;
        public Category CategoryProduct
        {
            get => _categoryProduct;
            set { _categoryProduct = value; 
                OnPropertyChanged(nameof(CategoryProduct)); }
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
            set
            {
                _quantityProduct = value; OnPropertyChanged(nameof(QuantityProduct));
            }
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


        // пользователи
        public static string UserLogin { get; set; }
        public static string UserPassword { get; set; }
        public static int UserPhoneNumber { get; set; }

        // производители
        public static string ManufacturerName { get; set; }

        // Свойство для изображения
        private byte[] _imageData;
        public byte[] ImageData
        {
            get => _imageData;
            set
            {
                _imageData = value;
                ImageD = value;
                OnPropertyChanged(nameof(ImageData)); // Уведомляем о изменении
            }
        }

        
        

        // свойства для выделенных элементов
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
                    image.Freeze(); // Для безопасности в многопоточной среде
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
                    string resultStr = "Не выбран продукт";
                    if (SelectedProduct != null)
                    {
                        resultStr = DataWorker.EditProduct(
                            SelectedProduct,
                            CategoryProduct,
                            ManufacturerProduct,
                            ProductName,
                            QuantityProduct,
                            PriceProduct.ToString(),
                            DescriptionProduct,
                            ImageData); // Добавляем передачу изображения

                        UpdateAllDataView();
                        ShowMessageToUser(resultStr);
                        window.Close();
                        SetNullValuesToProperties();
                    }
                    else
                    {
                        ShowMessageToUser(resultStr);
                    }
                });
            }
        }


        #endregion

        #region COMMANDS_TO_ADD


        private RelayCommand addNewProduct { get; set; }
        public RelayCommand AddNewProduct
        {
            get
            {
                return addNewProduct ?? new RelayCommand((obj) =>
                {
                    Window window = obj as Window;
                    
                    string resultStr = "";

                    resultStr = DataWorker.CreateProduct(CategoryProduct, ManufacturerProduct, ProductName, QuantityProduct, PriceProduct, DescriptionProduct, ImageData);
                    UpdateAllDataView();
                    ShowMessageToUser(resultStr);
                    SetNullValuesToProperties();
                    window.Close();
                });
            }
        }

        #endregion

        #region COMMANDS_TO_DELETE
        private RelayCommand deleteItem { get; set; }
        public RelayCommand DeleteItem
        {
            get
            {
                return deleteItem ?? new RelayCommand(obj =>
                {
                    string resultStr = "Ничего не выбрано";

                    // удаление пользователь
                    if (SelectedTabItem.Name == "UsersTab" && SelectedUser != null)
                    {
                        resultStr = DataWorker.DeleteUser(SelectedUser);
                        UpdateAllDataView();
                    }
                    // удаление продукт
                    if (SelectedTabItem.Name == "ProductsTab" && SelectedProduct != null)
                    {
                        resultStr = DataWorker.DeleteProduct(SelectedProduct);
                        UpdateAllDataView();
                    }
                    // удаление отзыва
                    if (SelectedTabItem.Name == "ReviewsTab" && SelectedReview != null)
                    {
                        resultStr = DataWorker.DeleteReview(SelectedReview);
                        UpdateAllDataView();
                    }
                    // удаление заказа
                    if (SelectedTabItem.Name == "PurchasesTab" && SelectedPurchase != null)
                    {
                        resultStr = DataWorker.DeletePurchase(SelectedPurchase);
                        UpdateAllDataView();
                    }

                    ShowMessageToUser(resultStr);
                    SetNullValuesToProperties();
                }
                );
            }
        }
        #endregion


        #region COMMANDS_OPEN_WINDOWS

        // команда для открытия окна продукта
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


        // команда для редактирования элемента
        private RelayCommand openEditItem;
        public RelayCommand OpenEditItem
        {
            get
            {
                return openEditItem ?? new RelayCommand(obj =>
                {
                   
                    // удаление продукт
                    if (SelectedTabItem.Name == "ProductsTab" && SelectedProduct != null)
                    {
                        OpenEditProductWindow(SelectedProduct);
                    }

                    SetNullValuesToProperties();
                });
            }
        }
        #endregion


        #region METHODS_OPEN_WINDOW
        // методы открытия окон
        // добавление

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
            // категории
            CategoryName = null;
            // продукты
            CategoryProduct = null;
            ProductName = null;
            QuantityProduct = 0;
            PriceProduct = 0;
            DescriptionProduct = null;
            // пользователи

            UserLogin = null;
            UserPassword = null;
            UserPhoneNumber = 0;

            // производители
            ManufacturerProduct = null;

        }

        private void UpdateAllDataView()
        {
            UpdateAllCategoriesView();
            UpdateAllProductsView();
            UpdateAllUsersView();
            UpdateAllManufacturerView();
            UpdateAllReviewView();
            UpdateAllPurchasesView();
        }
        private void UpdateAllCategoriesView()
        {
            AllCategories = DataWorker.GetAllCategories();
            AdminView.AllCategoriesView.ItemsSource = null;
            AdminView.AllCategoriesView.Items.Clear();
            AdminView.AllCategoriesView.ItemsSource = AllCategories;
            AdminView.AllCategoriesView.Items.Refresh();
        }

        private void UpdateAllManufacturerView()
        {
            AllManufacturers = DataWorker.GetAllManufacturers();
            AdminView.AllManufacturersView.ItemsSource = null;
            AdminView.AllManufacturersView.Items.Clear();
            AdminView.AllManufacturersView.ItemsSource = AllManufacturers;
            AdminView.AllManufacturersView.Items.Refresh();
        }

        private void UpdateAllProductsView()
        {
            AllProducts = DataWorker.GetAllProducts();
            AdminView.AllProductsView.ItemsSource = null;
            AdminView.AllProductsView.Items.Clear();
            AdminView.AllProductsView.ItemsSource = AllProducts;
            AdminView.AllProductsView.Items.Refresh();
        }

        private void UpdateAllUsersView()
        {
            AllUsers = DataWorker.GetAllUsers();
            AdminView.AllUsersView.ItemsSource = null;
            AdminView.AllUsersView.Items.Clear();
            AdminView.AllUsersView.ItemsSource = AllUsers;
            AdminView.AllUsersView.Items.Refresh();
        }

        private void UpdateAllReviewView()
        {
            AllReviews = DataWorker.GetAllReviews();
            AdminView.AllReviewsView.ItemsSource = null;
            AdminView.AllReviewsView.Items.Clear();
            AdminView.AllReviewsView.ItemsSource = AllReviews;
            AdminView.AllReviewsView.Items.Refresh();
        }

        private void UpdateAllPurchasesView()
        {
            AllPurchases = DataWorker.GetAllPurchases();
            AdminView.AllPurchasesView.ItemsSource = null;
            AdminView.AllPurchasesView.Items.Clear();
            AdminView.AllPurchasesView.ItemsSource = AllPurchases;
            AdminView.AllPurchasesView.Items.Refresh();
        }
        #endregion        
    }
}
