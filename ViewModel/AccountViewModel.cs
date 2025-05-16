using AutoMarket.Model;
using AutoMarket.Model.Data;
using AutoMarket.View;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic;
using SharpVectors.Dom;
using System.Windows.Media;
using System.Windows.Controls;

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
                OnPropertyChanged(nameof(BalanceDisplay));
            }
        }

        public string BalanceDisplay => $"{CurrentUser?.Balance ?? 0} BYN";

        public ObservableCollection<PurchaseViewModel> PendingPurchases { get; } = new();
        public ObservableCollection<PurchaseViewModel> ConfirmedPurchases { get; } = new();
        public ObservableCollection<Product> PurchasedProducts { get; } = new();
        public ObservableCollection<ProductReviewViewModel> ProductReviews { get; } = new();

        public ICommand BackToMainCommand { get; }
        public ICommand TopUpBalanceCommand { get; }
        public ICommand ConfirmPurchaseCommand { get; }
        public ICommand CancelPurchaseCommand { get; }
        private ICommand _submitReviewCommand;
        public ICommand SubmitReviewCommand => _submitReviewCommand ??= new RelayCommand(SubmitReview);

        public AccountViewModel(User user)
        {
            CurrentUser = user;

            BackToMainCommand = new RelayCommand(_ => BackToMain());
            TopUpBalanceCommand = new RelayCommand(_ => TopUpBalance());
            _submitReviewCommand = new RelayCommand(SubmitReview);
            ConfirmPurchaseCommand = new RelayCommand(ConfirmPurchase);
            CancelPurchaseCommand = new RelayCommand(CancelPurchase);

            LoadPurchasedProducts();
            LoadPurchases();
        }

        private void LoadPurchases()
        {
            LoadPendingPurchases();
            LoadConfirmedPurchases();
        }

        private void LoadPendingPurchases()
        {
            try
            {
                if (CurrentUser?.Id == null) return;

                PendingPurchases.Clear();
                var pending = DataWorker.GetPendingPurchases(CurrentUser.Id);
                foreach (var p in pending)
                    PendingPurchases.Add(new PurchaseViewModel(p));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void LoadConfirmedPurchases()
        {
            try
            {
                if (CurrentUser?.Id == null) return;

                ConfirmedPurchases.Clear();
                var confirmed = DataWorker.GetConfirmedPurchasesByUserId(CurrentUser.Id);
                foreach (var p in confirmed)
                {
                    var vm = new PurchaseViewModel(p);
                    vm.LoadProduct();
                    ConfirmedPurchases.Add(vm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void LoadPurchasedProducts()
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
                ProductReviews.Add(new ProductReviewViewModel
                {
                    Product = product,
                    HasUserReviewed = DataWorker.UserHasReviewedProduct(CurrentUser.Id, product.Id)
                });
            }
        }

        private void ConfirmPurchase(object parameter)
        {
            if (parameter is PurchaseViewModel purchaseVM)
            {
                purchaseVM.Purchase.Status = Purchase.PurchaseStatus.Confirmed;

                if (DataWorker.ConfirmPurchase(purchaseVM.Purchase))
                {
                    PendingPurchases.Remove(purchaseVM);
                    ConfirmedPurchases.Add(purchaseVM);
                    LoadPurchasedProducts();
                    MessageBox.Show("Заказ подтвержден и перемещен в архив!");
                }
                else
                {
                    MessageBox.Show("Ошибка при подтверждении заказа.");
                }
            }
        }

        private void CancelPurchase(object parameter)
        {
            if (parameter is PurchaseViewModel purchaseVM)
            {
                if (DataWorker.CancelPurchase(purchaseVM.Purchase.Id))
                {
                    CurrentUser.Balance += purchaseVM.TotalPrice;
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(BalanceDisplay));
                    PendingPurchases.Remove(purchaseVM);
                    MessageBox.Show("Заказ отменен. Деньги возвращены на баланс.");
                }
            }
        }

        private void SubmitReview(object parameter)
        {
            if (parameter is PurchaseViewModel purchaseVM)
            {
                try
                {
                    // Проверка данных
                    if (string.IsNullOrWhiteSpace(CurrentUser.Login))
                    {
                        MessageBox.Show("Не указано имя пользователя");
                        return;
                    }

                    if (purchaseVM.HasUserReviewed)
                    {
                        MessageBox.Show("Вы уже оставили отзыв на этот товар.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(purchaseVM.Comment) || purchaseVM.Comment.Length < 10)
                    {
                        MessageBox.Show("Отзыв должен содержать минимум 10 символов.");
                        return;
                    }

                    if (purchaseVM.Rating < 1 || purchaseVM.Rating > 5)
                    {
                        MessageBox.Show("Пожалуйста, поставьте оценку от 1 до 5 звезд.");
                        return;
                    }

                    using (var context = new ApplicationContext())
                    {
                        var review = new Review
                        {
                            UserId = CurrentUser.Id,
                            ProductId = purchaseVM.Product.Id,
                            AuthorName = CurrentUser.Login,
                            Comment = purchaseVM.Comment.Trim(),
                            Rating = purchaseVM.Rating,
                            DateCreated = DateTime.Now
                        };

                        context.Reviews.Add(review);
                        context.SaveChanges();

                        // Обновляем состояние
                        purchaseVM.HasUserReviewed = true;

                        MessageBox.Show("Спасибо за ваш отзыв!");

                        // Обновляем список отзывов
                        LoadPurchasedProducts();
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    string errorMessage = "Ошибка сохранения отзыва: ";
                    errorMessage += dbEx.InnerException?.Message ?? dbEx.Message;
                    MessageBox.Show(errorMessage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
                }
            }
        }


        private void TopUpBalance()
        {
            var inputDialog = new InputDialog("Пополнение баланса", "Введите сумму для пополнения:");
            if (inputDialog.ShowDialog() == true)
            {
                if (decimal.TryParse(inputDialog.Answer, out decimal amount) && amount > 0)
                {
                    // Обновляем баланс пользователя
                    bool success = DataWorker.UpdateUserBalance(CurrentUser.Id, amount);

                    if (success)
                    {
                        CurrentUser.Balance += amount;
                        OnPropertyChanged(nameof(CurrentUser));
                        OnPropertyChanged(nameof(BalanceDisplay));
                        MessageBox.Show($"Баланс успешно пополнен на {amount} BYN");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при пополнении баланса");
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму (положительное число)");
                }
            }
        }

        private void BackToMain()
        {
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

    public class InputDialog
    {
        public string Title { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        public InputDialog(string title, string question)
        {
            Title = title;
            Question = question;
        }

        public bool? ShowDialog()
        {
            Window window = new Window()
            {
                Title = Title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            StackPanel panel = new StackPanel() { Margin = new Thickness(10) };

            panel.Children.Add(new TextBlock() { Text = Question, Margin = new Thickness(0, 0, 0, 10) });

            TextBox textBox = new TextBox();
            panel.Children.Add(textBox);

            Button okButton = new Button()
            {
                Content = "OK",
                Width = 70,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            okButton.Click += (sender, e) =>
            {
                Answer = textBox.Text;
                window.DialogResult = true;
            };

            panel.Children.Add(okButton);

            window.Content = panel;
            return window.ShowDialog();
        }
    }
}