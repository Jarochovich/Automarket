using AutoMarket.Model;
using AutoMarket.Model.Data;
using AutoMarket.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class AccountViewModel : BaseViewModel
    {
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        private ObservableCollection<Product> _purchasedProducts;
        public ObservableCollection<Product> PurchasedProducts
        {
            get => _purchasedProducts;
            set
            {
                _purchasedProducts = value;
                OnPropertyChanged(nameof(PurchasedProducts));
            }
        }

        public ICommand BackToMainCommand { get; }
        public ObservableCollection<ProductReviewViewModel> ProductReviews { get; set; }

        public ICommand SubmitReviewCommand { get; }

        public AccountViewModel(User user)
        {
            CurrentUser = user;
            PurchasedProducts = new ObservableCollection<Product>();
            BackToMainCommand = new RelayCommand(_ => BackToMain());
            SubmitReviewCommand = new RelayCommand(SubmitReview);

            // Загрузка данных о покупках
            ProductReviews = new ObservableCollection<ProductReviewViewModel>();
            LoadPurchasedProducts();
        }

        private void LoadPurchasedProducts()
        {
            try
            {
                if (CurrentUser?.Id == null)
                {
                    MessageBox.Show("Пользователь не авторизован");
                    return;
                }

                PurchasedProducts.Clear();
                ProductReviews.Clear();

                var products = DataWorker.GetPurchasedProducts(CurrentUser.Id);

                if (products == null || !products.Any())
                {
                    MessageBox.Show("У вас пока нет покупок");
                    return;
                }

                foreach (var product in products)
                {
                    PurchasedProducts.Add(product);
                    var reviewVM = new ProductReviewViewModel { Product = product };

                    // Проверяем, оставлял ли пользователь отзыв на этот продукт
                    reviewVM.HasUserReviewed = DataWorker.UserHasReviewedProduct(CurrentUser.Id, product.Id);

                    ProductReviews.Add(reviewVM);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки покупок: {ex.Message}");
                Debug.WriteLine($"Полная ошибка: {ex}");
            }
        }

        private void SubmitReview(object obj)
        {
            if (obj is ProductReviewViewModel reviewVM)
            {
                if (reviewVM.HasUserReviewed)
                {
                    MessageBox.Show("Вы уже оставили отзыв на этот товар.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(reviewVM.Comment))
                {
                    MessageBox.Show("Пожалуйста, напишите отзыв.");
                    return;
                }
                MessageBox.Show($"User: {CurrentUser.Id}, Product: {reviewVM.Product?.Id}, Rating: {reviewVM.Rating}, Comment: {reviewVM.Comment}");
                bool success = DataWorker.AddReview(CurrentUser.Id, reviewVM.Product.Id, reviewVM.Comment, reviewVM.Rating);
                if (success)
                {
                    MessageBox.Show("Отзыв успешно добавлен.");
                    reviewVM.Comment = string.Empty;
                    reviewVM.Rating = 0;
                    reviewVM.HasUserReviewed = true;
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении отзыва.");
                }
            }
        }


        private void BackToMain()
        {

            // Закрываем текущее окно
            foreach (Window window in Application.Current.Windows)
            {
                if (window is AccountView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}