using Microsoft.Win32;
using RELOD_Tools.Logic;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RELOD_Tools.PriceList
{
    public class DoPriceList
    {
        protected delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        public DoPriceList(string exceptionsList, bool? fullPrice)
        {
            List<PriceModel> priceList  = new List<PriceModel>();
            CultureInfo culture         = CultureInfo.CreateSpecificCulture("en-US");
            exceptionsList              = exceptionsList.Replace("\r", "");
            string[] exceptions         = exceptionsList.Split('\n');
            string[] fileText           = WorkWithFile.OpenFile();
            string[,] price;

            // Проверяем, был ли выбран файл, если нет то прерываем программу
            if (fileText == null)
            {
                return;
            }
            //==================================================================================================

            // Нужно подсчитать количество знаков табуляции. Так мы поймем сколько будет столбцов у будущего массива "price"
            int rows    = fileText.GetUpperBound(0);
            int columns = 0;
            string str  = fileText[0];
            string tab  = "\t";
            int index   = 0; ;

            while ((index = str.IndexOf(tab, index)) != -1)
            {
                columns++;
                index = index + tab.Length;
            }
            price = new string[rows, columns];
            //==================================================================================================

            // Заполняем массив данными
            for (int i = 0; i < rows; i++)
            {
                string[] temp = fileText[i].Split('\t');
                for (int j = 0; j < columns; j++)
                {
                    price[i, j] = temp[j];
                }
            }
            //==================================================================================================

            // Проверяем на нулевые цены ("0.00") и исключаем их, если таковые находятся
            for (int i = 0; i < rows; i++)
            {
                if (price[i, 5] == "0.00")
                {
                    price[i, 0] = "0";
                }
            }
            //==================================================================================================

            // Блок проверки наименований на наличие. 
            // Наименования с нулевым количеством на складах (учитываются склад Северянин, Пушкарев и магазин) не будут попадать в прайс-лист
            string zero = "0.00";
            price[0, 0] = "0";

            if (fullPrice == false)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (price[i, 7] == zero && price[i, 9] == zero && price[i, 11] == zero)
                    {
                        price[i, 0] = "0";
                    }
                }
            }
            // Это условие срабатывает если пользователь поставил галку в чек боксе "Полный прайс"
            else
            {
                string op = "OP!";
                string na = "NA!";
                for (int i = 0; i < rows; i++)
                {
                    if (price[i, 2].EndsWith(op) || price[i, 2].EndsWith(na) && price[i, 7] == zero && price[i, 9] == zero && price[i, 11] == zero)
                    {
                        price[i, 0] = "0";
                    }
                }
            }
            //==================================================================================================

            // Блок проверки наименований на "агентское вознаграждение". 
            // Агентское соглашение не будет попадать в прайс-лист
            string agent = "агентское вознаграждение";

            for (int i = 0; i < rows; i++)
            {
                if (price[i, 14] != null && price[i, 14].ToLower().StartsWith(agent))
                {
                    price[i, 0] = "0";
                }
            }
            //==================================================================================================

            // Блок проверки групп товаров. 
            // Если группа товаров равна группе из списка исключений, то такое наименование не будет попадать в прайс
            if (fullPrice == false)
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < exceptions.Length; j++)
                    {
                        if (price[i, 3] == exceptions[j])
                        {
                            price[i, 0] = "0";
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < exceptions.Length; j++)
                    {
                        if (price[i, 3] == exceptions[j])
                        {
                            price[i, 0] = "0";
                        }
                    }
                    if (price[i, 3] == "RELOD Ltd. (RUR)" || price[i, 3] == "RELOD LTD." && price[i, 7] == "0.00" && price[i, 9] == "0.00" && price[i, 11] == "0.00")
                    {
                        price[i, 0] = "0";
                    }
                }
            }
            //==================================================================================================

            // Блок переноса данных из массива price в итоговый priceList
            
            for (int i = 0; i < rows; i++)
            {
                if (price[i, 0] != "0")
                {
                    string warehouse    = "0";
                    string store        = "0";

                    if ((double.Parse(price[i, 7], culture) + double.Parse(price[i, 11], culture)) > 10)
                    {
                        warehouse = "Более 10 шт";
                    }
                    else
                    {
                        warehouse = (double.Parse(price[i, 7], culture) + double.Parse(price[i, 11], culture)).ToString();
                    }

                    if (double.Parse(price[i, 9], culture) > 10)
                    {
                        store = "Более 10 шт";
                    }
                    else
                    {
                        store = double.Parse(price[i, 9], culture).ToString();
                    }

                    priceList.Add(new PriceModel
                    {
                        ISBN            = price[i, 1],                                  // присваиваем ISBN
                        Title           = price[i, 14],                                 // присваиваем Наименование
                        Price           = double.Parse(price[i, 6], culture),//.ToString(), // присваиваем Цену
                        VAT             = double.Parse(price[i, 4], culture),//.ToString(), // присваиваем НДС
                        Group           = price[i, 3],                                  // присваиваем Группу
                        QTYwarehouse    = warehouse,                                    // присваиваем Количество на складах (Северянин + Пушкарев)
                        QTYstore        = store,                                        // присваиваем Количество в магазине
                        ShortTitle      = price[i, 2]                                   // присваиваем Краткое наименование
                    });
                }
            }

            // Сортируем наш прайс по полю ShortTitle
            priceList = priceList.OrderBy(item => item.ShortTitle).ToList();

            // Добавляем нумерацию
            int count = 1;
            foreach (PriceModel item in priceList)
            {
                item.Number = count; //.ToString();
                count++;
            }

            SaveAsExcel(priceList);
        }
        private void SaveAsExcel(List<PriceModel> priceList)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage excelPackage   = new ExcelPackage();
            ExcelWorksheet worksheet    = excelPackage.Workbook.Worksheets.Add(DateTime.Now.ToString("dd.MM.yyyy"));

            // Добавляем шапку в первую строку
            worksheet.Cells["A1"].Value = "#";
            worksheet.Cells["B1"].Value = "ISBN";
            worksheet.Cells["C1"].Value = "Наименование товара";
            worksheet.Cells["D1"].Value = "Цена с НДС";
            worksheet.Cells["E1"].Value = "НДС";
            worksheet.Cells["F1"].Value = "Группа товара";
            worksheet.Cells["G1"].Value = "Кол-во на складе";
            worksheet.Cells["H1"].Value = "Кол-во в магазине";
            worksheet.Cells["I1"].Value = "Краткое наименование";

            // Добавляем данные из priceList начиная со второй строки
            worksheet.Cells["A2"].LoadFromCollection(priceList);

            // Устанавливаем ширину столбцов, кроме последнего ("Краткое наименование")
            worksheet.Column(1).AutoFit();      // #
            worksheet.Column(2).Width = 16;     // ISBN
            worksheet.Column(3).Width = 110;    // Наименование товара
            worksheet.Column(4).Width = 13;     // Цена с НДС
            worksheet.Column(5).Width = 7;      // НДС
            worksheet.Column(6).Width = 22;     // Группа товара
            worksheet.Column(7).Width = 19;     // Кол-во на складе
            worksheet.Column(8).Width = 19;     // Кол-во в магазине
            worksheet.Column(9).Width = 24;     // краткое наименование

            worksheet.Column(4).Style.Numberformat.Format   = "0.00";
            worksheet.View.FreezePanes(2,1);
            worksheet.Cells["A1:I1"].Style.Font.Bold        = true;
            worksheet.Cells["A1:I1"].AutoFilter             = true;
            worksheet.Cells["A1:I" + (priceList.Count + 1)].Style.Border.Top.Style      = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:I" + (priceList.Count + 1)].Style.Border.Right.Style    = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:I" + (priceList.Count + 1)].Style.Border.Bottom.Style   = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:I" + (priceList.Count + 1)].Style.Border.Left.Style     = ExcelBorderStyle.Thin;

            // Сохраняем файл
            SaveFileDialog sfd  = new SaveFileDialog();
            string fileName     = "Price roznitca " + DateTime.Now.ToString("dd.MM.yyyy");
            sfd.Title           = "Сохранить прайс-лист ...";
            sfd.DefaultExt      = ".xlsx";
            sfd.FileName        = fileName;
            sfd.Filter          = "Ecxel (*.xlsx) | *.xlsx";

            if (sfd.ShowDialog() == true)
            {
                FileInfo fi = new FileInfo(sfd.FileName);
                excelPackage.SaveAs(fi);
            }
        }
    }
}
