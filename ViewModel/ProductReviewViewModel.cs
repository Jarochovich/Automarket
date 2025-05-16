using AutoMarket.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class ProductReviewViewModel : BaseViewModel
    {
        public Product Product { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public bool HasUserReviewed { get; set; }
        public ObservableCollection<StarItem> Stars { get; set; } = new ObservableCollection<StarItem>();

        public ICommand SetRatingCommand => new RelayCommand(obj =>
        {
            if (obj is int value)
            {
                Rating = value;
                UpdateStars();
            }
        });

        public ProductReviewViewModel()
        {
            InitializeStars();
        }

        private void InitializeStars()
        {
            Stars.Clear();
            for (int i = 1; i <= 5; i++)
            {
                Stars.Add(new StarItem { Value = i, IsFilled = false });
            }
        }

        private void UpdateStars()
        {
            foreach (var star in Stars)
            {
                star.IsFilled = star.Value <= Rating;
            }
        }
    }
}