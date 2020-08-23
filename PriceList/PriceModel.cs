using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RELOD_Tools.PriceList
{
    public class PriceModel
    {
        public int Number { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public double VAT { get; set; }
        public string Group { get; set; }
        public string QTYwarehouse { get; set; }
        public string QTYstore { get; set; }
        public string ShortTitle { get; set; }
    }
}
