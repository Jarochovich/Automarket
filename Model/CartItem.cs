using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int CountItem { get; set; }

        public decimal TotalPrice => Product.Price * CountItem;
    }
}
