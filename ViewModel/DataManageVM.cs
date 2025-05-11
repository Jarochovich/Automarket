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

namespace AutoMarket.ViewModel
{
    class DataManageVM : BaseViewModel
    {
        public ICommand LoadImageCommand { get; }
        public ICommand LogoutCommand { get; }
        // языки
        public ICommand SetRussianCommand { get; }
        public ICommand SetEnglishCommand { get; }

        public DataManageVM()
        {
            LoadImageCommand = new RelayCommand(param => LoadImage());
            LogoutCommand = new RelayCommand(_ => Logout());

            SetRussianCommand = new RelayCommand(_ => App.ChangeLanguage("ru"));
            SetEnglishCommand = new RelayCommand(_ => App.ChangeLanguage("en"));
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
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
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

        // категория
        public static string CategoryName { get; set; }

        // продукт
        public static Category CategoryProduct { get; set; }
        public static Manufacturer ManufacturerProduct { get; set; }
        public static string ProductName { get; set; }
        public static decimal PriceProduct { get; set; }
        public static string descriptionProduct { get; set; }
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
        public TabItem SelectedTabItem { get; set; }
        public static User SelectedUser { get; set; }
        public static Category SelectedCategory { get; set; }
        public static Manufacturer SelectedManufacturer { get; set; }
        public static Product SelectedProduct { get; set; }


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
                            ManufacturerProduct.Id,
                            ProductName,
                            PriceProduct,
                            descriptionProduct,
                            ImageData); // Добавляем передачу изображения

                        UpdateAllDataView();
                        SetNullValuesToProperties();
                        ShowMessageToUser(resultStr);
                        window.Close();
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
                    if (CategoryProduct == null)
                    {
                        ShowMessageToUser("Укажите категорию товара");
                    }
                    if (ProductName == null || ProductName.Replace(" ", "").Length == 0)
                    {
                        //SetRedBlockControll(window, "ProductName");
                    }
                    if (PriceProduct == 0)
                    {
                        //SetRedBlockControll(window, "Price");
                    }
                    if (descriptionProduct == null || descriptionProduct.Replace(" ", "").Length == 0)
                    {
                        //SetRedBlockControll(window, "Description");
                    }
                    if (ManufacturerProduct == null)
                    {
                        ShowMessageToUser("Укажите категорию товара");

                    }
                    else
                    {
                        resultStr = DataWorker.CreateProduct(CategoryProduct, ManufacturerProduct, ProductName, PriceProduct, descriptionProduct, ImageData);
                        UpdateAllDataView();
                        ShowMessageToUser(resultStr);
                        SetNullValuesToProperties();
                        window.Close();
                    }
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
                    // удаление категория
                    if (SelectedTabItem.Name == "CategoriesTab" && SelectedCategory != null)
                    {
                        resultStr = DataWorker.DeleteCategory(SelectedCategory);
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
            PriceProduct = 0;
            descriptionProduct = null;
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
        #endregion

        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            SetCenterPositionAndOpen(messageView);
        }

        
    }
}
