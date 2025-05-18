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

            var viewModel = new DataManageVM();
            DataContext = viewModel;

            viewModel.SelectedProduct = product;
            viewModel.ProductName = product.Name;
            viewModel.QuantityProduct = product.Quantity;
            viewModel.PriceProduct = product.Price;
            viewModel.DescriptionProduct = product.Description;
            viewModel.ImageData = product.ImageData;

            viewModel.CategoryProduct = viewModel.AllCategories
                .FirstOrDefault(c => c.Id == product.Category?.Id);

            viewModel.ManufacturerProduct = viewModel.AllManufacturers
                .FirstOrDefault(m => m.Id == product.Manufacturer?.Id);
        }
    }
}

