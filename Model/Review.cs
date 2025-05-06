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
        public string Author { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
