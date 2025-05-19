using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMarket.Model.Data;
using System.Linq;
using AutoMarket.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using static AutoMarket.Model.Purchase;
using System.IO.Pipelines;
using System.Globalization;

namespace AutoMarket.Model
{
    public static class DataWorker
    {
        // получить все категории
        public static List<Category>  GetAllCategories()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Categories.ToList();
                return results;
            }
        }

        // получить всех производителей
        public static List<Manufacturer> GetAllManufacturers()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Manufacturers.ToList();
                return results;
            }
        }

        // получить все продукты
        public static List<Product> GetAllProducts()
        {
            using (var db = new ApplicationContext())
            {
                return db.Products
                  .Include(p => p.Manufacturer)
                  .Include(p => p.Category) // Добавляем загрузку категории
                  .ToList();
            }
        }

        public static List<Review> GetAllReviews()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Reviews.ToList();
                return results;
            }
        }

        

        // Списание товара
        public static int DecreaseProductQuantity(int productId, int count)
        {
            using (var context = new ApplicationContext())
            {
                var product = context.Products.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    product.Quantity -= count;
                    if (product.Quantity < 0)
                    {
                        product.Quantity = 0;
                    }

                    context.SaveChanges();
                    return product.Quantity; // Возвращаем обновленное количество
                }
                return -1; // Или бросить исключение, если продукт не найден
            }
        }



        // Возврат на склад
        public static bool CancelPurchase(int purchaseId)
        {
            try
            {
                using (var db = new ApplicationContext())
                {
                    var purchase = db.Purchases.FirstOrDefault(p => p.Id == purchaseId);
                    if (purchase == null || purchase.Status != Purchase.PurchaseStatus.Pending)
                        return false;

                    purchase.Status = Purchase.PurchaseStatus.Canceled;

                    var product = db.Products.FirstOrDefault(p => p.Id == purchase.ProductId);
                    if (product != null)
                    {
                        product.Quantity += purchase.Quantity; // Возвращаем товар на склад
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }



        // получить продукты конкретной категории
        public static List<Product> GetProductsByCategory(int categoryId)
        {
            using (var db = new ApplicationContext())
            {
                return db.Products
                         .Include(p => p.Manufacturer)
                         .Where(p => p.CategoryId == categoryId)
                         .ToList();
            }
        }

        // получить отзывы по продукту
        public static List<Review> GetReviewsByProductId(int productId)
        {
            using (var context = new ApplicationContext())
            {
                return context.Reviews
                              .Include(r => r.User) // загружаем автора
                              .Where(r => r.ProductId == productId)
                              .Select(r => new Review
                              {
                                  Id = r.Id,
                                  Comment = r.Comment,
                                  Rating = r.Rating,
                                  ProductId = r.ProductId,
                                  UserId = r.UserId,
                                  AuthorName = r.User.Login,
                                  DateCreated = r.DateCreated
                              })
                              .ToList();
            }
        }


        // получить всех пользователей
        public static List<User> GetAllUsers()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Users.ToList();
                return results;
            }
        }

        public static Product GetProductById(int productId)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    return context.Products
                        .Include(p => p.Manufacturer)
                        .FirstOrDefault(p => p.Id == productId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении продукта: {ex.Message}");
                return null;
            }
        }

        // проверка на администратора
        public static bool IsAdmin(string login, string password)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user != null)
                {
                    // Сравниваем хеш пароля
                    string hashedPassword = Hashing.HashPassword(password, user.PasswordSalt); // Сначала добавляем соль к паролю
                    if (user.PasswordHash == hashedPassword)
                    {
                        // Если логин совпадает с администраторским
                        return login == "admin";
                    }
                }
            }
            return false;
        }

        // Получить ожидающие покупки пользователя
        public static List<Purchase> GetPendingPurchases(int userId)
        {
            using (var db = new ApplicationContext())
            {
                return db.Purchases
                    .Where(p => p.UserId == userId && p.Status == Purchase.PurchaseStatus.Pending)
                    .ToList();
            }
        }

        public static List<Product> GetConfirmedPurchases(int userId)
        {
            using (var db = new ApplicationContext())
            {
                return db.Purchases
                    .Include(p => p.Product)
                    .Where(p => p.UserId == userId && p.Status == PurchaseStatus.Confirmed)
                    .Select(p => p.Product)
                    .ToList();
            }
        }


        // Получить все заказы
        public static List<Purchase> GetAllPurchases()
        {
            using (var db = new ApplicationContext())
            {
                return db.Purchases
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToList();
            }
        }


        public static bool ConfirmPurchase(Purchase purchase)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                   var dbPurchase = context.Purchases.Find(purchase.Id);
                   if (dbPurchase == null) return false;

                   dbPurchase.Status = Purchase.PurchaseStatus.Confirmed;
                   context.SaveChanges();
                   return true;
                }  
            }
            catch
            {
                return false;
            }
        }


        public static List<Purchase> GetConfirmedPurchasesByUserId(int userId)
        {
            using (var context = new ApplicationContext())
            {
                return context.Purchases
                    .Where(p => p.UserId == userId && p.Status == Purchase.PurchaseStatus.Confirmed)
                    .ToList();
            }
        }


        public static Purchase SavePendingPurchase(Purchase purchase)
        {
            using (var db = new ApplicationContext())
            {
                db.Purchases.Add(purchase);
                db.SaveChanges();
                return purchase;
            }
        }

        // получить конкретного пользователя
        public static bool GetUser(string login, string password)
        {
            using var db = new ApplicationContext();

            var user = db.Users.FirstOrDefault(u => u.Login == login);
            if (user == null) return false;

            string hash = Hashing.HashPassword(password, user.PasswordSalt);
            return hash == user.PasswordHash;
        }

        // получить пользователя по логину
        public static User GetUserByLogin(string login)
        {
            using var db = new ApplicationContext();
            return db.Users.FirstOrDefault(u => u.Login == login);
        }

        public static List<Product> GetPurchasedProducts(int userId)
        {
            try
            {
                using (var db = new ApplicationContext())
                {
                    var purchases = db.Purchases
                        .Where(p => p.UserId == userId)
                        .Include(p => p.Product)
                            .ThenInclude(prod => prod.Manufacturer)
                        .Include(p => p.Product)
                            .ThenInclude(prod => prod.Category)
                        .Include(p => p.Product)
                            .ThenInclude(prod => prod.Reviews)
                        .AsNoTracking()
                        .ToList(); // Выполняем запрос здесь

                    // Теперь в C# задаём PurchaseQuantity
                    var products = purchases.Select(p =>
                    {
                        p.Product.PurchaseQuantity = p.Quantity;
                        return p.Product;
                    }).ToList();

                    return products;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке покупок: {ex}");
                throw;
            }
        }


        public static bool UserHasReviewedProduct(int userId, int productId)
        {
            using (var context = new ApplicationContext())
            {
                return context.Reviews.Any(r => r.UserId == userId && r.ProductId == productId);
            }
        }
        public static int GetProductQuantity(int productId)
        {
            using (var context = new ApplicationContext())
            {
                // Отключаем кеширование для этого запроса
                var product = context.Products
                    .AsNoTracking() // Не кешировать сущность
                    .FirstOrDefault(p => p.Id == productId);

                return product?.Quantity ?? 0;
            }
        }



        // добавить новый продукт
        public static string CreateProduct(Category category, Manufacturer manufacturer, string name, int quantity, decimal price, string description, byte[] imageData = null)
        {
            // Валидация входных параметров
            if (category == null)
                return "Не указана категория продукта";

            if (manufacturer == null)
                return "Не указан производитель";

            if (string.IsNullOrWhiteSpace(name))
                return "Не указано название продукта";

            if (name.Length < 2)
                return "Название продукта должно содержать не менее 2 символов";

            if (price <= 0)
                return "Цена должна быть больше нуля";

            if (quantity <= 0)
                return "Количество товаров должно быть больше нуля";

            if (string.IsNullOrWhiteSpace(description))
                return "Не указано описание продукта";

            if (description.Length < 10)
                return "Описание товара должно содержать не менее 10 символов";

            if (imageData == null)
                return "Не добавлено изображение товара";

            string result = "Продукт уже существует";
            using (ApplicationContext db = new ApplicationContext())
            {
                try
                {
                    // проверка на существование (учитываем только имя и цену, так как другие параметры могут повторяться)
                    bool checkIsExist = db.Products.Any(el => el.Name == name && el.Price == price && el.CategoryId == category.Id);
                    if (!checkIsExist)
                    {
                        Product newProduct = new Product
                        {
                            CategoryId = category.Id,
                            ManufacturerId = manufacturer.Id,
                            Name = name.Trim(),
                            Quantity = quantity,
                            Price = price,
                            Description = description.Trim(),
                            ImageData = imageData
                        };

                        db.Products.Add(newProduct);
                        db.SaveChanges();
                        result = "Продукт успешно добавлен!";
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при добавлении продукта: {ex.Message}");
                    return $"Ошибка при добавлении продукта: {ex.Message}";
                }
            }
        }

        // пополнить баланс
        public static bool UpdateUserBalance(int userId, decimal amount)
        {
            try
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        user.Balance += amount;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении баланса: {ex.Message}");
                return false;
            }
        }

        // Добавить пользователя (асинхронно)
        public static async Task<bool> CreateUserAsync(string login, string password, string phone)
        {
            await using var db = new ApplicationContext();
            if (await db.Users.AnyAsync(u => u.Login == login))
                return false;

            string salt = Hashing.GenerateSalt();
            string hash = Hashing.HashPassword(password, salt);

            await db.Users.AddAsync(new User
            {
                Login = login,
                PasswordSalt = salt,
                PasswordHash = hash,
                PhoneNumber = phone
            });

            await db.SaveChangesAsync();
            return true;
        }


        // удалить отзыв
        public static string DeleteReview(Review review)
        {
            string result = "Такого отзыва нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                result = $"Комментарий {review.Comment} успешно удален!";
            }
            return result;
        }

        // удалить заказ
        public static string DeletePurchase(Purchase purchase)
        {
            string result = "Такого заказа нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                db.Purchases.Remove(purchase);
                db.SaveChanges();
                result = $"Заказ пользователя {purchase.User} успешно удален!";
            }
            return result;
        }


        // удалить продукт
        public static string DeleteProduct(Product product)
        {
            string result = "Такого продукта нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                db.Products.Remove(product);
                db.SaveChanges();
                result = $"Продукт {product.Name} успешно удален!";
            }
            return result;
        }

        // удалить пользователя
        public static string DeleteUser(User user)
        {
            string result = "Такого пользователя нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                db.Users.Remove(user);
                db.SaveChanges();
                result = $"Пользователь {user.Login} успешно удален!";
            }
            return result;
        }

        

        public static string EditProduct(Product oldProduct, Category newCategory, Manufacturer newManufacturer, string newName, int newQuantity, string newPriceStr, string newDescription, byte[] newImageData)
        {
            // Валидация
            if (newCategory == null)
                return "Не указана категория продукта";

            if (newManufacturer == null)
                return "Не указан производитель";

            if (string.IsNullOrWhiteSpace(newName))
                return "Не указано название продукта";

            if (newName.Length < 2)
                return "Название продукта должно содержать не менее 2 символов";

            if (newQuantity <= 0)
                return "Не указано количество продукта";

            // Улучшенная проверка цены
            if (string.IsNullOrWhiteSpace(newPriceStr))
                return "Цена должна быть указана";

            // Нормализация строки с ценой
            string normalizedPrice = newPriceStr.Trim()
                                              .Replace(" ", "") // Удаляем пробелы
                                              .Replace(",", "."); // Заменяем запятые на точки

            if (!decimal.TryParse(normalizedPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal newPrice) || newPrice <= 0)
                return "Цена должна быть числом больше нуля";

            if (string.IsNullOrWhiteSpace(newDescription))
                return "Не указано описание продукта";

            if (newDescription.Length < 10)
                return "Описание товара должно содержать не менее 10 символов";

            if (newImageData == null || newImageData.Length == 0)
                return "Не добавлено изображение товара";

            // Обновление
            using (ApplicationContext db = new ApplicationContext())
            {
                Product product = db.Products.FirstOrDefault(p => p.Id == oldProduct.Id);
                if (product == null)
                    return "Продукт не найден!";

                product.Category = newCategory;
                product.ManufacturerId = newManufacturer.Id;
                product.Name = newName;
                product.Quantity = newQuantity;
                product.Price = newPrice;
                product.Description = newDescription;
                product.ImageData = newImageData;

                db.SaveChanges();
                return $"Продукт \"{product.Name}\" успешно изменён!";
            }
        }

 
    }
}
