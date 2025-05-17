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

        // получить максимальную цену продукта
        public static decimal GetMaxPriceByProduct()
        {
            using (var db = new ApplicationContext())
            {
                return db.Products
                         .Max(p => p.Price);
            }
        }














        public static int GetFilteredProductCount(int? categoryId, int? manufacturerId,
        decimal minPrice, decimal maxPrice, string searchText)
        {
            using (var context = new ApplicationContext())
            {
                var query = context.Products.AsQueryable();

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (manufacturerId.HasValue)
                    query = query.Where(p => p.ManufacturerId == manufacturerId.Value);

                query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

                if (!string.IsNullOrEmpty(searchText))
                    query = query.Where(p => p.Name.Contains(searchText));

                return query.Count();
            }
        }

        public static List<Product> GetFilteredProducts(int? categoryId, int? manufacturerId,
            decimal minPrice, decimal maxPrice, string searchText,
            int pageSize, int skip)
        {
            using (var context = new ApplicationContext())
            {
                var query = context.Products
                    .Include(p => p.Manufacturer)
                    .Include(p => p.Category)
                    .AsQueryable();

                if (categoryId.HasValue)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (manufacturerId.HasValue)
                    query = query.Where(p => p.ManufacturerId == manufacturerId.Value);

                query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

                if (!string.IsNullOrEmpty(searchText))
                    query = query.Where(p => p.Name.Contains(searchText));

                return query
                    .OrderBy(p => p.Id)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();
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

        // проверка баланса пользователя
        public static bool ProcessPayment(int userId, decimal amount)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = db.Users.FirstOrDefault(u => u.Id == userId);
                        if (user == null) return false;

                        if (user.Balance < amount) return false;

                        user.Balance -= amount;
                        db.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static Product GetProductById(int productId)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    return context.Products
                        .Include(p => p.Manufacturer) // Если нужно загрузить связанного производителя
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
                        return login == "admin"; // Можно добавить более гибкую логику, если нужно
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

        // Обновить статус покупки
        public static bool UpdatePurchaseStatus(int purchaseId, PurchaseStatus status)
        {
            using (var db = new ApplicationContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var purchase = db.Purchases.FirstOrDefault(p => p.Id == purchaseId);
                        if (purchase == null) return false;

                        purchase.Status = status;
                        db.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
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




        // Получить архивные покупки (подтвержденные и отмененные)
        public static List<Purchase> GetArchivedPurchases(int userId)
        {
            using (var db = new ApplicationContext())
            {
                return db.Purchases
                    .Include(p => p.Product)
                    .Where(p => p.UserId == userId &&
                           (p.Status == PurchaseStatus.Confirmed || p.Status == PurchaseStatus.Canceled))
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
                    try
                    {
                        var dbPurchase = context.Purchases.Find(purchase.Id);
                        if (dbPurchase == null) return false;

                        dbPurchase.Status = Purchase.PurchaseStatus.Confirmed;
                        context.SaveChanges();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Можно залогировать ошибку, если надо
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


        public static void SaveConfirmedPurchase(Purchase purchase)
        {
            using (var db = new ApplicationContext())
            {
                db.Purchases.Add(purchase);
                db.SaveChanges();
            }
        }

        public static bool CancelPurchase(int purchaseId)
        {
            using (var db = new ApplicationContext())
            {
                var purchase = db.Purchases.FirstOrDefault(p => p.Id == purchaseId);
                if (purchase == null) return false;

                purchase.Status = Purchase.PurchaseStatus.Canceled;
                db.SaveChanges();
                return true;
            }
        }


        // Добавить новую покупку (уже есть SavePurchase, но можно добавить статус по умолчанию)
        public static void SavePurchase(Purchase purchase)
        {
            using var db = new ApplicationContext();
            purchase.Status = PurchaseStatus.Pending; // Устанавливаем статус по умолчанию
            db.Purchases.Add(purchase);
            db.SaveChanges();
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



        public static bool AddReview(int userId, int productId, string comment, int rating)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var user = context.Users.Find(userId);
                    var product = context.Products.Find(productId);

                    if (user == null || product == null)
                        return false;

                    var review = new Review
                    {
                        UserId = userId,
                        ProductId = productId,
                        Comment = comment,
                        Rating = rating,
                        DateCreated = DateTime.Now,
                        AuthorName = user.Login // !!! ВАЖНО: заполняем обязательное поле
                    };

                    context.Reviews.Add(review);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string error = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка при сохранении: {error}");
                return false;
            }
        }

        public static bool UserHasReviewedProduct(int userId, int productId)
        {
            using (var context = new ApplicationContext())
            {
                return context.Reviews.Any(r => r.UserId == userId && r.ProductId == productId);
            }
        }

        public static Review GetUserReview(int userId, int productId)
        {
            using (var context = new ApplicationContext())
            {
                return context.Reviews.FirstOrDefault(r => r.UserId == userId && r.ProductId == productId);
            }
        }

        // создать категорию
        public static string CreateCategory(string category)
        {
            string result = "Категория уже существует";
            using (ApplicationContext db = new ApplicationContext())
            {
                // проверка на существование
                bool checkIsExist = db.Categories.Any(el => el.Name == category);
                if (!checkIsExist)
                {
                    Category newCategory = new Category { Name = category };
                    db.Categories.Add(newCategory);
                    db.SaveChanges();
                    result = "Категория добавлена!";
                }
                return result;
            }
        }

        // добавить новый продукт
        public static string CreateProduct(Category category, Manufacturer manufacturer, string name, decimal price, string description, byte[] imageData = null)
        {
            // Валидация входных параметров
            if (category == null)
                return "Не указана категория продукта";

            if (manufacturer == null)
                return "Не указан производитель";

            if (string.IsNullOrWhiteSpace(name))
                return "Не указано название продукта";

            if (price <= 0)
                return "Цена должна быть больше нуля";

            if (string.IsNullOrWhiteSpace(description))
                return "Не указано описание продукта";

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

        // удалить категорию
        public static string DeleteCategory(Category category)
        {
            string result = "Такой категории нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                db.Categories.Remove(category);
                db.SaveChanges();
                result = $"Категория {category.Name} успешно удалена!";
            }
            return result;
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

        // изменить категорию
        public static string EditCategory(Category oldCategory, string newName)
        {
            string result = "Такой категории нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                Category category = db.Categories.FirstOrDefault(cat => cat.Id == oldCategory.Id);
                if (category != null)
                {
                    category.Name = newName;
                    db.SaveChanges();
                    result = $"Категория успешно изменена c {oldCategory.Name} на {category.Name}!";
                }    
            }
            return result;
        }

        // изменить продукт
        public static string EditProduct(Product oldProduct, Category newCategoryName, int newManufacturerId, string newName, decimal newPrice, string newDescription, byte[] newImageData)
        {
            string result = "Такого продукта нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                Product product = db.Products.FirstOrDefault(prod => prod.Id == oldProduct.Id);
                if (product != null)
                {
                    product.Category = newCategoryName;
                    product.ManufacturerId = newManufacturerId;
                    product.Name = newName;
                    product.Price = newPrice;
                    product.Description = newDescription;
                    product.ImageData = newImageData;
                    db.SaveChanges();
                    result = $"Продукт {oldProduct.Name} успешно изменен!";
                }
            }
            return result;
        }

        // изменить пользователя
        public static string EditUser(User oldUser, string newLogin, string newPassword, string newPhone)
        {
            string result = "Такого пользователя нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                User user = db.Users.FirstOrDefault(us => us.Id == oldUser.Id);
                if (user != null)
                {
                    // Обновляем логин и телефон
                    user.Login = newLogin;
                    user.PhoneNumber = newPhone;

                    // Если пароль изменился, хешируем его и сохраняем в базу
                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        // Генерация новой соли и хеша пароля
                        var salt = Hashing.GenerateSalt();
                        var passwordHash = Hashing.HashPassword(newPassword, salt);

                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = salt;
                    }

                    db.SaveChanges();
                    result = $"Пользователь {oldUser.Login} успешно изменен!";
                }
            }
            return result;
        }
    }
}
