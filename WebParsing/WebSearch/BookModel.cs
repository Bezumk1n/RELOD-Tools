using System;

namespace RELOD_Tools.WebSearch
{
    public class BookModel
    {
        public string Number { get; set; }
        public string Isbn { get; set; }
        public string Isbn2 { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string PubDate { get; set; }
        public string Publisher { get; set; }
        public string Imprint { get; set; }
        public string Supplier { get; set; }
        public string PriceWithCurrency { get; set; }
        public string Price { get; set; }
        public string PriceComparision { get; set; } // для PubEasy
        public string Discount { get; set; }
        public string Availability { get; set; }
        public string Availability2 { get; set; }
        public string MarketRestrictions { get; set; } // для PubEasy
        public string Readership { get; set; }
        public string Edition { get; set; }
        public string Weight { get; set; }
        public string Dimensions { get; set; }
        public string PubCountry { get; set; }
        public string Classification { get; set; }
        public string BookCover { get; set; }
        public string Pages { get; set; }
        public string Series { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string Contents { get; set; }
        public string Length { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
    }
}
