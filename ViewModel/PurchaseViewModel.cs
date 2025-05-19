using AutoMarket.Model;
using AutoMarket.Model.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class PurchaseViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private Product _product;
        private string _comment;
        private int _rating;
        private bool _hasUserReviewed;

        public Purchase Purchase { get; set; }

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(ProductImage));
            }
        }

        public string ProductName => Product?.Name ?? "Неизвестный товар";
        public byte[] ProductImage => Product?.ImageData;
        public decimal TotalPrice => (Purchase?.PriceAtPurchase ?? 0) * (Purchase?.Quantity ?? 0);
        public string PurchaseDate => Purchase?.PurchaseDate.ToString("dd.MM.yyyy");

        public bool HasUserReviewed
        {
            get => _hasUserReviewed;
            set
            {
                _hasUserReviewed = value;
                OnPropertyChanged(nameof(HasUserReviewed));
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public int Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged(nameof(Rating));
                UpdateStars();
            }
        }

        public ObservableCollection<StarItem> Stars { get; } = new ObservableCollection<StarItem>();

        public ICommand SetRatingCommand => new RelayCommand(obj =>
        {
            if (obj is int value)
                Rating = value;
        });

        public PurchaseViewModel(Purchase purchase)
        {
            Purchase = purchase;
            InitializeStars();
            LoadProduct();
        }

        public void LoadProduct()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    // продукт с производителем
                    Product = context.Products
                        .Include(p => p.Manufacturer)
                        .AsNoTracking()
                        .FirstOrDefault(p => p.Id == Purchase.ProductId);

                    // наличие отзыва
                    var review = context.Reviews
                        .AsNoTracking()
                        .FirstOrDefault(r => r.UserId == Purchase.UserId && r.ProductId == Purchase.ProductId);

                    if (review != null)
                    {
                        HasUserReviewed = true;
                        Comment = review.Comment;
                        Rating = review.Rating;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessageToUser($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void InitializeStars()
        {
            Stars.Clear();
            for (int i = 1; i <= 5; i++)
            {
                Stars.Add(new StarItem
                {
                    Value = i,
                    IsFilled = i <= Rating
                });
            }
        }



        public void UpdateStars()
        {
            foreach (var star in Stars)
            {
                star.IsFilled = star.Value <= Rating;
            }
            OnPropertyChanged(nameof(Stars));
        }

        public void RefreshReviewStatus()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    HasUserReviewed = context.Reviews
                        .Any(r => r.UserId == Purchase.UserId && r.ProductId == Purchase.ProductId);
                }
            }
            catch (Exception ex)
            {
                ShowMessageToUser($"Ошибка обновления статуса отзыва: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}