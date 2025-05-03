using AutoMarket.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для AddNewUserView.xaml
    /// </summary>
    public partial class AddNewUserView : Window
    {
        public AddNewUserView()
        {
            InitializeComponent();
            DataContext = new DataManageVM();
        }

        private void PasswordUser_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is DataManageVM)
            {
                DataManageVM.UserPassword = PasswordUser.Password;
            }
        }
    }
}
