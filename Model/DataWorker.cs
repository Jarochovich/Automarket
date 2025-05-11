using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMarket.Model.Data;
using System.Linq;
using AutoMarket.Helpers;
using Microsoft.EntityFrameworkCore;

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
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Reviews.Where(r => r.ProductId == productId).ToList();
                return results;
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

        // получить конкретного пользователя
        public static bool GetUser(string login, string password)
        {
            using var db = new ApplicationContext();

            var user = db.Users.FirstOrDefault(u => u.Login == login);
            if (user == null) return false;

            string hash = Hashing.HashPassword(password, user.PasswordSalt);
            return hash == user.PasswordHash;
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

        // добавить продукт
        public static string CreateProduct(Category category, Manufacturer manufacturer, string name, decimal price, string description, byte[] imageData = null)
        {
            string result = "Продукт уже существует";
            using (ApplicationContext db = new ApplicationContext())
            {
                // проверка на существование
                bool checkIsExist = db.Products.Any(el => el.Name == name && el.Price == price);
                if (!checkIsExist)
                {
                    Product newProduct = new Product { CategoryId = category.Id, ManufacturerId = manufacturer.Id, Name = name, Price = price, Description = description, ImageData = imageData };
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                    result = "Продукт добавлен!";
                }
                return result;
            }
        }

        // добавить пользователя
        public static bool CreateUser(string login, string password, string phone)
        {
            using var db = new ApplicationContext();

            if (db.Users.Any(u => u.Login == login)) return false;

            string salt = Hashing.GenerateSalt();
            string hash = Hashing.HashPassword(password, salt);

            var user = new User
            {
                Login = login,
                PasswordSalt = salt,
                PasswordHash = hash,
                PhoneNumber = phone
            };

            db.Users.Add(user);
            db.SaveChanges();
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
