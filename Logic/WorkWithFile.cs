using System;
using RELOD_Tools.WebSearch;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;


namespace RELOD_Tools.Logic
{
    static class WorkWithFile
    {
        public static string CheckForExceptionsFileExistance(string exceptionsDirectory, string exceptionsFilePath)
        {
            string exceptions = string.Empty;

            if (File.Exists(exceptionsFilePath))
            {
                exceptions = File.ReadAllText(exceptionsFilePath, Encoding.UTF8);
            }
            else
            {
                Directory.CreateDirectory(exceptionsDirectory);
                File.Create(exceptionsFilePath);
            }
            return exceptions;
        }
        public static void SaveFile(List<BookModel> book)
        {
            SaveFileDialog sfd  = new SaveFileDialog();
            string fileName     = DateTime.Now.ToString("yyyy-MM-dd HH-mm") + ".txt";
            sfd.Title           = "Сохранить результат поиска ...";
            sfd.DefaultExt      = ".txt";
            sfd.FileName        = fileName;
            sfd.Filter          = "Текстовый файл (*.txt) | *.txt";

            if (sfd.ShowDialog() == true)
            {
                Stream s        = File.Create(sfd.FileName);
                StreamWriter sw = new StreamWriter(s, Encoding.Unicode);
                foreach (BookModel bm in book)
                {
                    sw.Write(bm.Number + '\t');
                    sw.Write(bm.Isbn + '\t');
                    sw.Write(bm.Isbn2 + '\t');
                    sw.Write(bm.Title + '\t');
                    sw.Write(bm.Author + '\t');
                    sw.Write(bm.PubDate + '\t');
                    sw.Write(bm.Publisher + '\t');
                    sw.Write(bm.Imprint + '\t');
                    sw.Write(bm.Supplier + '\t');
                    sw.Write(bm.PriceWithCurrency + '\t');
                    sw.Write(bm.Price + '\t');
                    sw.Write(bm.PriceComparision + '\t');
                    sw.Write(bm.Discount + '\t');
                    sw.Write(bm.Availability + '\t');
                    sw.Write(bm.Availability2 + '\t');
                    sw.Write(bm.MarketRestrictions + '\t');
                    sw.Write(bm.Readership + '\t');
                    sw.Write(bm.Edition + '\t');
                    sw.Write(bm.Weight + '\t');
                    sw.Write(bm.Dimensions + '\t');
                    sw.Write(bm.PubCountry + '\t');
                    sw.Write(bm.Classification + '\t');
                    sw.Write(bm.BookCover + '\t');
                    sw.Write(bm.Pages + '\t');
                    sw.Write(bm.Series + '\t');
                    sw.Write(bm.Description + '\t');
                    sw.Write(bm.Language + '\t');
                    sw.Write(bm.Contents + '\t');
                    sw.Write(bm.Length + '\t');
                    sw.Write(bm.Width + '\t');
                    sw.Write(bm.Height + '\t');
                    sw.WriteLine();
                }
                sw.Close();
            }
        }
        public static string[] OpenFile()
        {
            OpenFileDialog ofd  = new OpenFileDialog();
            ofd.Filter          = "Текстовый файл (*.txt) | *.txt";
            ofd.ShowDialog();

            string fileName     = ofd.FileName;
            string[] fileText   = null;

            if (fileName != string.Empty)
            {
                fileText = File.ReadAllLines(fileName, Encoding.UTF8);
                return fileText;
            }

            return fileText;
        }
        
        public static void CreateDataBase(string dbName, string dbPath)
        {
            if (!File.Exists(dbPath + dbName))
                {
                SQLiteConnection.CreateFile(dbPath + dbName);
                SQLiteConnection connection = new SQLiteConnection("Data Source = " + dbPath + dbName);
                connection.Open();

                SQLiteCommand cmd   = new SQLiteCommand(connection);
                cmd.CommandText     = @"CREATE TABLE IF NOT EXISTS codes (id INTEGER PRIMARY KEY,
                    code TEXT, date TEXT)";
                cmd.ExecuteNonQuery();

                connection.Close();
            }
            
        }
    }
}
