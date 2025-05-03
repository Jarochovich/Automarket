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

namespace AutoMarket.ViewModel
{
    class DataManageVM : INotifyPropertyChanged
    {
        // все категории
        private List<Category> allCategories = DataWorker.GetAllCategories();
        public List<Category> AllCategories
        {
            get { return allCategories; }
            set { 
                allCategories = value;
                NotifyPropertyChanged("AllCategories");
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
                NotifyPropertyChanged("AllProducts");
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
                NotifyPropertyChanged("AllUsers");
            }
        }

        // категория
        public static string CategoryName { get; set; }

        // продукт
        public static Category CategoryProduct { get; set; }
        public static string ProductName { get; set; }
        public static decimal PriceProduct { get; set; }
        public static string descriptionProduct { get; set; }

        // пользователи
        public static string UserLogin { get; set; }
        public static string UserPassword { get; set; }
        public static decimal UserPhoneNumber { get; set; }

        // свойства для выделенных элементов
        public TabItem SelectedTabItem { get; set; }
        public static User SelectedUser { get; set; }
        public static Category SelectedCategory { get; set; }
        public static Product SelectedProduct { get; set; }

        #region COMMANDS_TO_EDIT
        private RelayCommand editUser { get; set; }
        public RelayCommand EditUser
        {
            get
            {
                return editUser ?? new RelayCommand(obj =>
                {
                    Window window = obj as Window;
                    string resultStr = "Не выбран пользователь";
                    if (SelectedUser != null)
                    {
                        resultStr = DataWorker.EditUser(SelectedUser, UserLogin, UserPassword, UserPhoneNumber);
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

        private RelayCommand editProduct { get; set; }
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
                        resultStr = DataWorker.EditProduct(SelectedProduct, CategoryProduct.Id, ProductName, PriceProduct, descriptionProduct);
                        UpdateAllDataView();
                        SetNullValuesToProperties();
                        ShowMessageToUser(resultStr);
                        window.Close();
                    }
                    else
                    {
                        ShowMessageToUser(resultStr);
                    }
                }
                );
            }
        }

        private RelayCommand editCategory { get; set; }
        public RelayCommand EditCategory
        {
            get
            {
                return editCategory ?? new RelayCommand(obj =>
                {
                    Window window = obj as Window;
                    string resultStr = "Не выбрана категория";
                    if (SelectedProduct != null)
                    {
                        resultStr = DataWorker.EditCategory(SelectedCategory, CategoryName);
                        UpdateAllDataView();
                        SetNullValuesToProperties();
                        ShowMessageToUser(resultStr);
                        window.Close();
                    }
                    else
                    {
                        ShowMessageToUser(resultStr);
                    }
                }
                );
            }
        }
        #endregion

        #region COMMANDS_TO_ADD

        private RelayCommand addNewCategory { get; set; }
        public RelayCommand AddNewCategory
        {
            get
            {
                return addNewCategory ?? new RelayCommand((obj) =>
                {
                    Window window = obj as Window;
                    string resultStr = "";
                    if (CategoryName == null || CategoryName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControll(window, "CategoryTextBox");
                    }
                    else
                    {
                        resultStr = DataWorker.CreateCategory(CategoryName);
                        UpdateAllDataView();
                        ShowMessageToUser(resultStr);
                        SetNullValuesToProperties();
                        window.Close();
                    }
                });
            }  
        }

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
                        MessageBox.Show("Укажите категорию товара");
                    }
                    if (ProductName == null || ProductName.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControll(window, "ProductName");
                    }
                    if (PriceProduct == 0)
                    {
                        SetRedBlockControll(window, "Price");
                    }
                    if (descriptionProduct == null || descriptionProduct.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControll(window, "Description");
                    }
                    else
                    {
                        resultStr = DataWorker.CreateProduct(CategoryProduct, ProductName, PriceProduct, descriptionProduct);
                        UpdateAllDataView();
                        ShowMessageToUser(resultStr);
                        SetNullValuesToProperties();
                        window.Close();
                    }
                });
            }
        }

        private RelayCommand addNewUser { get; set; }
        public RelayCommand AddNewUser
        {
            get
            {
                return addNewUser ?? new RelayCommand((obj) =>
                {
                    Regex regex = new Regex("^(?=.+[A-Za-z])(?=.+\\d)(?=.+[$@$!%*#?&])[A-Za-z\\d$@$!%*#?&]{8,}$");
                    Window window = obj as Window;
                    string resultStr = "";
                    if (UserIsAdmin == null)
                    {
                        MessageBox.Show("Укажите тип пользователя");
                    }
                    if (UserLogin == null || UserLogin.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControll(window, "NameUser");
                    }
                    if (UserPassword == null || UserPassword.Replace(" ", "").Length == 0)
                    {
                        SetRedBlockControll(window, "NameUser");
                    }
                    if (regex.IsMatch(UserPassword))
                    {
                        MessageBox.Show("Все окей броооо");
                    }
                    if (UserPhoneNumber == 0)
                    {
                        SetRedBlockControll(window, "MoneyUser");
                    }
                    else
                    {
                        resultStr = DataWorker.CreateUser(UserLogin, UserPassword, UserPhoneNumber);
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
        // команда для открытия окна категории
        private RelayCommand openAddNewCategory;
        public RelayCommand OpenAddNewCategory
        {
            get {
                return openAddNewCategory ?? new RelayCommand(obj =>
                {
                    OpenAddCategoryWindow();
                });
            }
        }

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

        // команда для открытия окна пользователей
        private RelayCommand openAddNewUser;
        public RelayCommand OpenAddNewUser
        {
            get
            {
                return openAddNewUser ?? new RelayCommand(obj =>
                {
                    OpenAddUserWindow();
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
                    // удаление пользователь
                    if (SelectedTabItem.Name == "UsersTab" && SelectedUser != null)
                    {
                        OpenEditUserWindow(SelectedUser);
                        
                    }
                    // удаление продукт
                    if (SelectedTabItem.Name == "ProductsTab" && SelectedProduct != null)
                    {
                        OpenEditProductWindow(SelectedProduct);
                    }
                    // удаление категория
                    if (SelectedTabItem.Name == "CategoriesTab" && SelectedCategory != null)
                    {
                        OpenEditCategoryWindow(SelectedCategory);
                    }

                    SetNullValuesToProperties();
                });
            }
        }
        #endregion


        #region METHODS_OPEN_WINDOW
        // методы открытия окон
        // добавление
        private void OpenAddCategoryWindow()
        {
            AddNewCategoryView addCategoryWindow = new AddNewCategoryView();
            SetCenterPositionAndOpen(addCategoryWindow);
        }

        private void OpenAddProductWindow()
        {
            AddNewProductView addProductWindow = new AddNewProductView();
            SetCenterPositionAndOpen(addProductWindow);
        }

        private void OpenAddUserWindow()
        {
            AddNewUserView addUserWindow = new AddNewUserView();
            SetCenterPositionAndOpen(addUserWindow);
        }

        // редактирование 
        private void OpenEditCategoryWindow(Category category)
        {
            EditCategoryView editCategoryWindow = new EditCategoryView(category);
            SetCenterPositionAndOpen(editCategoryWindow);
        }

        private void OpenEditProductWindow(Product product)
        {
            EditProductView editProductWindow = new EditProductView(product);
            SetCenterPositionAndOpen(editProductWindow);
        }

        private void OpenEditUserWindow(User user)
        {
            EditUserView editUserWindow = new EditUserView(user);
            SetCenterPositionAndOpen(editUserWindow);
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

        }

        private void UpdateAllDataView()
        {
            UpdateAllCategoriesView();
            UpdateAllProductsView();
            UpdateAllUsersView();
        }
        private void UpdateAllCategoriesView()
        {
            AllCategories = DataWorker.GetAllCategories();
            AdminView.AllCategoriesView.ItemsSource = null;
            AdminView.AllCategoriesView.Items.Clear();
            AdminView.AllCategoriesView.ItemsSource = AllCategories;
            AdminView.AllCategoriesView.Items.Refresh();
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


        // вспомогательные функции
        private void SetRedBlockControll(Window window, string blockName)
        {
            Control block = window.FindName(blockName) as Control;
            block.BorderBrush = Brushes.Red;
        }

        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            SetCenterPositionAndOpen(messageView);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
