using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RELOD_Tools.Logic
{
    static class AlphabetCheck
    {
        public static string Check(string str)
        {
            //StringBuilder builder       = new StringBuilder(str);
            char[] stringToCharacters   = str.ToCharArray();

            char[,] alphabet =
                {
                    // Французские символы
                    { 'Â', 'A'},
                    { 'À', 'A'},
                    { 'â', 'A'},
                    { 'à', 'a'},
                    { 'È', 'E'},
                    { 'É', 'E'},
                    { 'Ê', 'E'},
                    { 'Ë', 'E'},
                    { 'é', 'e'},
                    { 'è', 'e'},
                    { 'ê', 'e'},
                    { 'ë', 'e'},
                    { 'Ù', 'U'},
                    { 'Û', 'U'},
                    { 'Ü', 'U'},
                    { 'ù', 'u'},
                    { 'û', 'u'},
                    { 'ü', 'u'},
                    { 'Ç', 'c'},
                    { 'ç', 'c'},
                    { 'Ÿ', 'Y'},
                    { 'ÿ', 'y'},
                    { 'Ï', 'I'},
                    { 'ï', 'i'},
                    { 'Î', 'I'},
                    { 'î', 'i'},
                    { 'Ô', 'O'},
                    { 'ô', 'o'},

                    // Немецкие символы
                    { 'Ä', 'A'},
                    { 'ä', 'a'},
                    { 'Ö', 'O'},
                    { 'ö', 'o'},
                    { 'Ü', 'U'},
                    { 'ü', 'u'},

                    // Прочие символы
                    { '’', '\''},
                    { '–', '-'},
                    { '»', '\''},
                    { '«', '\''}
                };

            for (int i = 0; i < stringToCharacters.Length; i++)
            {
                for (int j = 0; j < alphabet.GetUpperBound(0) + 1; j++)
                {
                    if (stringToCharacters[i] == alphabet[j, 0])
                    {
                        stringToCharacters[i] = alphabet[j, 1];
                    }
                }
            }

            str = new string(stringToCharacters);

            str = str.Replace("Æ", "AE");
            str = str.Replace("Œ", "OE");
            str = str.Replace("æ", "ae");
            str = str.Replace("œ", "oe");
            str = str.Replace("ß", "ss");
            str = str.Replace("ẞ", "Ss");
            str = str.Replace("--", " - ");
            str = str.Replace("&#39;", "'");
            str = str.Replace("&#34;", "'");
            str = str.Replace("&#039;", "'");
            str = str.Replace("&#145;", "'");
            str = str.Replace("&#146;", "'");
            str = str.Replace("&#150;", "-");
            str = str.Replace("&#151;", "-");
            str = str.Replace("&#174;", "®");
            str = str.Replace("&#183;", "-");
            str = str.Replace("&nbsp;", " ");
            str = str.Replace("&amp;", "&");
            str = str.Replace("&#8211;", "-");
            str = str.Replace("&#8212;", "-");
            str = str.Replace("&#8216;", "'");
            str = str.Replace("&#8217;", "'");
            str = str.Replace("&#8220;", "'");
            str = str.Replace("&#8221;", "'");
            str = str.Replace("&#8226;", "•");
            str = str.Replace("&#8230;", "...");
            str = str.Replace("&#8482;", "™");
            str = str.Replace("&#x3A;", ":");
            str = str.Replace("&#x25;", "%");
            str = str.Replace("&#x20;", " ");
            str = str.Replace("&lt;", "<");
            str = str.Replace("&gt;", ">");

            str = Regex.Replace(str, @"\s+", " "); // убирает все лишние пробелы и знаки переноса строк
            str = str.Trim();

            return str;
        }
    }
}
