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

        private bool _hasUserReviewed;
        public bool HasUserReviewed
        {
            get => _hasUserReviewed;
            set
            {
                _hasUserReviewed = value;
                OnPropertyChanged(nameof(HasUserReviewed));
            }
        }


        public Product Product { get; set; }
        public string Comment { get; set; }

        private int _rating;
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

        public ObservableCollection<StarItem> Stars { get; set; } = new();

        public ICommand SetRatingCommand => new RelayCommand(obj =>
        {
            if (obj is int value)
                Rating = value;
        });

        public ProductReviewViewModel()
        {
            UpdateStars();
        }

        private void UpdateStars()
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


    }
}