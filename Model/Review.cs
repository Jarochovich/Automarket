using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } // <-- важно
        public string AuthorName { get; set; } // <-- важно
        public int ProductId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; } // от 1 до 5
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
