using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMarket.Model;
using AutoMarket.View;

namespace AutoMarket.ViewModel
{ 
    public class MainViewModel
    {
        // Обработчики событий
        private void OpenProfile(object sender, MouseButtonEventArgs e)
        {
            RegistrationView profileWindow = new RegistrationView(); // Создание нового окна
            profileWindow.Show(); // Открытие нового окна
        }
    }
}