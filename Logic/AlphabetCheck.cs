using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RELOD_Tools.Logic
{
    static class AlphabetCheck
    {
        public static string Check(string str)
        {
            //StringBuilder builder       = new StringBuilder(str);./
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

            str = Regex.Replace(str, @"\s+", " "); // убирает все лишние пробелы и знаки переноса строк
            str = str.Trim();

            return str;
        }
    }
}
