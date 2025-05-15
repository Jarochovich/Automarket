using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.ViewModel
{
    public class StarItem : BaseViewModel
    {
        public int Value { get; set; }

        private bool _isFilled;
        public bool IsFilled
        {
            get => _isFilled;
            set
            {
                _isFilled = value;
                OnPropertyChanged(nameof(IsFilled));
            }
        }
    }
}
