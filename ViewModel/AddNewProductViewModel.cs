using AutoMarket.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;
using AutoMarket.View;

namespace AutoMarket.ViewModel
{
    public class AddProductViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Category> AllCategories { get; set; }
        public Category CategoryProduct { get; set; }

        public string ProductName { get; set; }
        public string DescriptionProduct { get; set; }
        public decimal PriceProduct { get; set; }

        private byte[] _imageData;
        public byte[] ImageData
        {
            get => _imageData;
            set
            {
                _imageData = value;
                OnPropertyChanged(nameof(ImagePreview));
            }
        }

        public BitmapImage ImagePreview
        {
            get
            {
                if (ImageData == null) return null;
                var image = new BitmapImage();
                using (var ms = new MemoryStream(ImageData))
                {
                    ms.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                }
                return image;
            }
        }

        public ICommand LoadImageCommand { get; }
        public ICommand SaveProductCommand { get; }

        public AddProductViewModel()
        {
            AllCategories = new ObservableCollection<Category>(DataWorker.GetAllCategories());
            LoadImageCommand = new RelayCommand(_ => LoadImage());
            SaveProductCommand = new RelayCommand(SaveProduct);
        }

        private void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png"
            };

            if (ofd.ShowDialog() == true)
            {
                ImageData = File.ReadAllBytes(ofd.FileName);
            }
        }

        private void SaveProduct(object window)
        {
            if (CategoryProduct == null || string.IsNullOrWhiteSpace(ProductName)) return;

            var result = DataWorker.CreateProduct(
                CategoryProduct,
                ProductName,
                PriceProduct,
                DescriptionProduct,
                ImageData
            );

            ShowMessageToUser(result);

            // Закрытие окна
            if (window is Window w) w.Close();
        }


        private void ShowMessageToUser(string message)
        {
            MessageView messageView = new MessageView
            {
                DataContext = new MessageViewModel(message)
            };
            SetCenterPositionAndOpen(messageView);
        }

        private void SetCenterPositionAndOpen(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
