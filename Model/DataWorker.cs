using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMarket.Model.Data;
using System.Linq;

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

        // получить все продукты
        public static List<Product> GetAllProducts()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var results = db.Products.ToList();
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
        public static string CreateProduct(Category category, string name, decimal price, string description)
        {
            string result = "Продукт уже существует";
            using (ApplicationContext db = new ApplicationContext())
            {
                // проверка на существование
                bool checkIsExist = db.Products.Any(el => el.Name == name && el.Price == price);
                if (!checkIsExist)
                {
                    Product newProduct = new Product { CategoryId = category.Id, Name = name, Price = price, Description = description };
                    db.Products.Add(newProduct);
                    db.SaveChanges();
                    result = "Продукт добавлен!";
                }
                return result;
            }
        }

        // добавить пользователя
        public static string CreateUser(string login, string password, decimal money)
        {
            string result = "Такой пользователь уже существует";
            using (ApplicationContext db = new ApplicationContext())
            {
                // проверка на существование
                bool checkIsExist = db.Users.Any(el => el.Login == login && el.Password == password);
                if (!checkIsExist)
                {
                    User newUser = new User { Login = login, Password = password, Money = money };
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    result = "Пользователь добавлен!";
                }
                return result;
            }
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
        public static string EditProduct(Product oldProduct, int newCategoryId, string newName, decimal newPrice, string newDescription)
        {
            string result = "Такого продукта нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                Product product = db.Products.FirstOrDefault(prod => prod.Id == oldProduct.Id);
                if (product != null)
                {
                    product.CategoryId = newCategoryId;
                    product.Name = newName;
                    product.Price = newPrice;
                    product.Description = newDescription;
                    db.SaveChanges();
                    result = $"Продукт {oldProduct.Name} успешно изменен!";
                }
            }
            return result;
        }

        // изменить пользователя
        public static string EditUser(User oldUser, string newLogin, string newPassword, decimal newMoney)
        {
            string result = "Такого пользователя нет!";

            using (ApplicationContext db = new ApplicationContext())
            {
                User user = db.Users.FirstOrDefault(us => us.Id == oldUser.Id);
                if (user != null)
                {
                    user.Login = newLogin;
                    user.Password = newPassword;
                    user.Money = newMoney;
                    db.SaveChanges();
                    result = $"Пользователь {oldUser.Login} успешно изменен!";
                }
            }
            return result;
        }
    }
}
