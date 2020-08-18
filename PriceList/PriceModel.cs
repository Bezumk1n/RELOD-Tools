using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RELOD_Tools.PriceList
{
    class PriceModel
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public string VAT { get; set; }
        public string Group { get; set; }
        public string QTYwarehouse { get; set; }
        public string QTYstore { get; set; }
        public string ShortTitle { get; set; }
    }
}
