using AutoMarket.Model;
using AutoMarket.ViewModel;
using System.Windows;

namespace AutoMarket.View
{
    /// <summary>
    /// Логика взаимодействия для ProductDetailView.xaml
    /// </summary>
    public partial class ProductDetailView : Window
    {
        public ProductDetailView(ProductDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
