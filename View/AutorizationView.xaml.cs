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
    /// Логика взаимодействия для AutorizationView.xaml
    /// </summary>
    public partial class AutorizationView : Window
    {
        public AutorizationView()
        {
            InitializeComponent();
            var viewModel = new AutorizationViewModel();
            viewModel.CloseAction = () => this.Close(); // Передаём действие закрытия

            this.DataContext = viewModel;

            // Устанавливаем ссылку на PasswordBox в ViewModel
            if (DataContext is AutorizationViewModel vm)
            {
                vm.PassBox = PassBox;
            }
        }

        private AutorizationViewModel ViewModel => (AutorizationViewModel)DataContext;

        private void FirstPassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ViewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
