using AutoMarket.Model;
using AutoMarket.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoMarket.View
{
    /// <summary>
    /// Логика взаимодействия для AdminView.xaml
    /// </summary>
    public partial class AdminView : Window
    {
        public static DataGrid AllCategoriesView;
        public static DataGrid AllProductsView;
        public static DataGrid AllUsersView;
        public static DataGrid AllManufacturersView;
        public AdminView()
        {
            InitializeComponent();
            DataContext = new DataManageVM();
            AllCategoriesView = ViewAllCategories;
            AllProductsView = ViewAllProducts;
            AllUsersView = ViewAllUsers;
            AllManufacturersView = ViewAllManufacturers;
        }
    }
}
