using AutoMarket.Model;
using AutoMarket.View;
using AutoMarket.ViewModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoMarket.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void ExecuteRefreshProducts(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RefreshProducts();
            }
        }

        private void CanExecuteRefreshProducts(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // Можно добавить логику активации
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RegistrationView authorizationView = new RegistrationView();
            authorizationView.Show();
        }

        private void ProductClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                // Получаем текущий DataContext (MainViewModel)
                if (DataContext is MainViewModel mainVM)
                {
                    // Передаем общий CartVM
                    var detailViewModel = new ProductDetailViewModel(product, mainVM.CartVM);
                    var detailWindow = new ProductDetailView(detailViewModel);
                    detailWindow.ShowDialog();
                }
            }
        }
    }
}