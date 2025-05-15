using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public byte[] ImageData { get; set; }

        // навигационное свойство для отзывов
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        public double Rating { get; set; }

        // Добавляем новое свойство (не сохраняемое в БД)
        [NotMapped] // Атрибут указывает, что это свойство не должно маппиться в БД
        public int PurchaseQuantity { get; set; }

    }
}
