using System;
using RELOD_Tools.WebSearch;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Net;
using System.IO.Compression;

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
                    sw.Write(bm.ImageUrl);
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
        public static void AddPriceToZIP(string destinationPath)
        {
            string temp             = @"\_temp\";                                                         // Имя временной папки, по завершении работы она будет удалена
            string directory        = destinationPath.Remove(destinationPath.LastIndexOf("\\"));          // Директория, в которой был сохранен прайс (берем путь, и из него удаляем имя файла)
            string fileName         = destinationPath.Substring(destinationPath.LastIndexOf("\\") + 1);   // Выделяем имя файла. Нужно чтобы скопировать файл во временную папку
            string startPath        = directory + temp;                                                   // Путь к архивируемой папке
            string zipPath          = directory + @"\relod_price.zip";                                    // Полный путь к выходному файлу
            string abbreviations    = @"\\Srv2008\relodobmen\Прайс-листы\Список сокращений.doc";          // Файл со списком сокращений

            // Создаем временную скрытую папку "_temp" и копируем туда прайс и файл "Список сокращений.doc"
            Directory.CreateDirectory(directory + temp);
            DirectoryInfo hideFolder = new DirectoryInfo(directory + temp);
            hideFolder.Attributes = FileAttributes.Hidden;

            File.Copy(destinationPath, directory + temp + fileName, true);
            File.Copy(abbreviations, directory + temp + @"\Список сокращений.doc", true);

            // Удаляем старый архив и создаем новый
            File.Delete(zipPath);
            ZipFile.CreateFromDirectory(startPath, zipPath);

            // Удаляем временную папку
            Directory.Delete(startPath, true);

            // Удаляем старый прайс-лист
            RemoveOldPrice(directory);

            // Загружаем архив на FTP
            UploadToFTP(zipPath, directory);
        }
        public static void UploadToFTP(string zipPath, string directory)
        {
            string[] login_pass = File.ReadAllLines(@"\\Srv2008\relodobmen\Прайс-листы\dailyUpload\log.txt", Encoding.UTF8);

            // Создаем объект FtpWebRequest - он указывает на файл, который будет создан
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.relod.nichost.ru/files/relod_price.zip");
            request.Credentials = new NetworkCredential(login_pass[0], login_pass[1]);

            // Устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Создаем поток для загрузки файла
            FileStream fs = new FileStream(zipPath, FileMode.Open);
            byte[] fileContents = new byte[fs.Length];
            fs.Read(fileContents, 0, fileContents.Length);
            fs.Close();

            // Пишем считанный в массив байтов файл в выходной поток
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
        }
        public static void RemoveOldPrice(string path)
        {
            string oldPrice = @"\Price roznitca " + DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy") + ".xlsx";
            if (File.Exists(path + oldPrice))
            {
                File.Delete(path + oldPrice);
            }
        }
    }
}
