using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Category Category { get; set; } // обязательно!
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] ImageData { get; set; }

        // Добавляем навигационное свойство для отзывов
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
