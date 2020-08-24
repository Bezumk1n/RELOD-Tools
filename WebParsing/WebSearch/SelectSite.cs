using RELOD_Tools.WebParsing.WebSearch.Site;
using RELOD_Tools.WebSearch.Site;

namespace RELOD_Tools.WebSearch
{
    static class SelectSite
    {
        public static void Search(string[] isbns, string webSite)
        {
            switch (webSite)
            {
                case "ABE-IPS":
                    new ABEIPS(isbns); // Gardners search = new Gardners(isbns);
                    break;
                case "American PubEasy":
                    new AmericanPubEasy(isbns);
                    break;
                case "PubEasy":
                    new PubEasy(isbns);
                    break;
                case "Gardners":
                    new Gardners(isbns);
                    break;
                case "Libri":
                    new Libri(isbns);
                    break;
                case "BookDepository":
                    new BookDepository(isbns);
                    break;
            }
        }
    }
}
