using RELOD_Tools.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RELOD_Tools.AuthorsCompare
{
    public class AuthorsComparer
    {
        public AuthorsComparer(string authorsForComare)
        {
            // Создаем массив с авторами для сравнения (пользовательский)
            authorsForComare = authorsForComare.Replace("\r", "");
            string[] authors = authorsForComare.Split('\n');

            // Создаем массив с авторами из файла (с этим массивом мы будем сравнивать новый массив авторов authors)
            string[] authorsSavedList = File.ReadAllLines("authors.txt");
            string[,] compareList = new string[authorsSavedList.Length, 3];

            // В ячейку compareList[i, 0] записываем исходные данные автора
            // В ячейку compareList[i, 1] записываем очищенные данные (без запятых, кавычек, пробелов и т.д)
            // В ячейку compareList[i, 2] записываем отсортированные данные
            for (int i = 0; i < authorsSavedList.Length; i++)
            {
                compareList[i, 0] = authorsSavedList[i];
                compareList[i, 1] = AlphabetCheck.Check(compareList[i, 0]);
                compareList[i, 1] = compareList[i, 1].Replace(" ", "");
                compareList[i, 1] = compareList[i, 1].Replace(",", "");
                compareList[i, 1] = compareList[i, 1].Replace("-", "");
                compareList[i, 1] = compareList[i, 1].Replace(".", "");
                compareList[i, 1] = compareList[i, 1].Replace("/", "");
                compareList[i, 1] = compareList[i, 1].Replace("\\", "");
                compareList[i, 1] = compareList[i, 1].Replace("'", "");
                compareList[i, 1] = compareList[i, 1].Replace("\"", "");
                compareList[i, 1] = compareList[i, 1].Replace("_", "");
                compareList[i, 1] = compareList[i, 1].ToLower();

                char[] temp = compareList[i, 1].ToCharArray();
                Array.Sort(temp);
                compareList[i, 2] = new string(temp);
            }
            //==================================================================================================

            List<AuthorsList> authorsList = new List<AuthorsList>();

            string tempAuthor;
            for (int i = 0; i < authors.Length; i++)
            {
                // Убираем все лишние символы, т.к. автор может быть написан как "Herbert Wells" так и "Wells, Herbert"
                tempAuthor = AlphabetCheck.Check(authors[i]);
                tempAuthor = tempAuthor.Replace(" ", "");
                tempAuthor = tempAuthor.Replace(",", "");
                tempAuthor = tempAuthor.Replace("-", "");
                tempAuthor = tempAuthor.Replace(".", "");
                tempAuthor = tempAuthor.Replace("/", "");
                tempAuthor = tempAuthor.Replace("\\", "");
                tempAuthor = tempAuthor.Replace("'", "");
                tempAuthor = tempAuthor.Replace("\"", "");
                tempAuthor = tempAuthor.Replace("_", "");
                tempAuthor = tempAuthor.ToLower();
                //==================================================================================================
                
                // В этой части разбиваем автора на символы и складываем хешсумму
                char[] temp = tempAuthor.ToCharArray();

                Array.Sort(temp);

                string sortedAuthor = "";

                for (int j = 0; j < compareList.GetUpperBound(0); j++)
                {
                    if ( new string(temp) == compareList[j, 2])
                    {
                        sortedAuthor = compareList[j, 0];
                    }
                }
                //==================================================================================================

                authorsList.Add(new AuthorsList 
                {
                    OriginAuthor        = authors[i],
                    SortedAuthorName    = tempAuthor,
                    NameInBitrix        = sortedAuthor,
                });
            }
            
            DataGrid DG             = new DataGrid();
            DG.Title                = "Таблица сравнения";
            DG.dataGrid.ItemsSource = authorsList;
            DG.Show();
        }
        private class AuthorsList
        { 
            public string OriginAuthor { get; set; }
            public string SortedAuthorName { get; set; }
            public string NameInBitrix { get; set; }
        }
    }
}
