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
    /// Логика взаимодействия для EditUserView.xaml
    /// </summary>
    public partial class EditUserView : Window
    {
        public EditUserView(User user)
        {
            InitializeComponent();
            DataContext = new DataManageVM();
            DataManageVM.SelectedUser = user;
            DataManageVM.UserLogin = user.Login;
            DataManageVM.UserPassword = user.Password;
            DataManageVM.UserPhoneNumber = user.Money; 
        }
    }
}
