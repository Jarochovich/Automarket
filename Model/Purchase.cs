using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class Purchase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending; // Новое поле

        // Добавим enum для статусов
        public enum PurchaseStatus
        {
            Pending = 0,    // Ожидает подтверждения
            Confirmed = 1,  // Подтвержден (Забираю)
            Canceled = 2   // Отменен (Отказаться)
        }
    }
}
