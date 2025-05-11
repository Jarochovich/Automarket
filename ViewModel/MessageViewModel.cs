using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoMarket.Model;
using System.Windows.Input;
using AutoMarket.View;

namespace AutoMarket.ViewModel 
{
    public class MessageViewModel : BaseViewModel
    {
        public ICommand CloseWindowCommand { get; }

        public MessageViewModel(string message)
        {
            CloseWindowCommand = new RelayCommand(CloseWindow);
            Message = message;
        }

        private string _message { get; set; }
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
            }
        }

        public void CloseWindow(object obj)
        {
            if (obj is Window window)
            {
                window.Close();
            }
        }
    }
}
