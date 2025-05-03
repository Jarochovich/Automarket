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
    /// Логика взаимодействия для EditProductView.xaml
    /// </summary>
    public partial class EditProductView : Window
    {
        public EditProductView(Product product)
        {
            InitializeComponent();
            DataContext = new DataManageVM();
            DataManageVM.SelectedProduct = product;
            DataManageVM.ProductName = product.Name;
            DataManageVM.PriceProduct = product.Price;
            DataManageVM.descriptionProduct = product.Description;
        }
    }
}
