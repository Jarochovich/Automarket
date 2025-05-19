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
    public class AddProductViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ObservableCollection<Category> AllCategories { get; set; }
        public Category CategoryProduct { get; set; }
        public Manufacturer ManufacturerProduct { get; set; }

        private int _quantityProduct;
        public int QuantityProduct
        {
            get => _quantityProduct;
            set
            {
                _quantityProduct = value;
                OnPropertyChanged(nameof(QuantityProduct));
            }
        }
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
                ManufacturerProduct,
                ProductName,
                QuantityProduct,
                PriceProduct,
                DescriptionProduct,
                ImageData
            );

            ShowMessageToUser(result);

            if (window is Window w) w.Close();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
