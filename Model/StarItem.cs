using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Model
{
    public class StarItem
    {
        public int Value { get; set; }
        public string Symbol => Value <= SelectedRating ? "★" : "☆";
        public static int SelectedRating { get; set; }
    }
}
