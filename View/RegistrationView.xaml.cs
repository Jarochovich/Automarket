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
    /// Логика взаимодействия для RegistrationView.xaml
    /// </summary>
    public partial class RegistrationView : Window
    {
        public RegistrationView()
        {
            InitializeComponent();
            var vm = new RegistrationViewModel();
            vm.CloseAction = new Action(() => this.Close());
            this.DataContext = vm;

            if (DataContext is RegistrationViewModel viewModel)
            {
                viewModel.FirstPassBox = FirstPassBox;
                viewModel.SecondPassBox = SecondPassBox;
            }
        }

        private RegistrationViewModel ViewModel => (RegistrationViewModel)DataContext;

        private void FirstPassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ViewModel.Password = ((PasswordBox)sender).Password;
        }
        
        private void SecondPassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ViewModel.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
 }
        