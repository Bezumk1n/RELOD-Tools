using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SQLite;

namespace RELOD_Tools.CodeGeneration
{
    public class CodeGen 
    {
        Random rnd          = new Random();
        private int qty     = 1; // количество генерируемых кодов по-умолчанию
        private int length  = 10;// длинна кода по-умолчанию

        public CodeGen(string codesQty, string codesLength, string mustStartrWith, string mustEndWith, string dbName, string dbPath)
        {
            bool check = CheckInput(codesQty, codesLength);
            if (check == true)
            {
                List<CodeModel> codesList = new List<CodeModel>();
                DataGrid DG = new DataGrid();

                SQLiteConnection connection = new SQLiteConnection("Data Source = " + dbPath + dbName);
                connection.Open();
                SQLiteCommand cmd = new SQLiteCommand(connection);

                for (int i = 0; i < qty; i++)
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH-mm");
                    string randomCode = mustStartrWith + GenerateCode(length) + mustEndWith;

                    cmd.CommandText = "INSERT INTO codes(code, date) VALUES(@code, @date)";
                    cmd.Parameters.AddWithValue("@code", randomCode);
                    cmd.Parameters.AddWithValue("@date", time);
                    cmd.ExecuteNonQuery();

                    codesList.Add(new CodeModel {Id = (i+1), Code = randomCode, Date = time });
                }

                connection.Close();

                DG.Title = "Сгенерированы коды: ";
                DG.Show();
                DG.dataGrid.ItemsSource = codesList;
            }
        }
        private bool CheckInput(string codesQty,string codesLength)
        {
            bool check = true;
            if (codesQty != "")
            {
                try
                {
                    qty = Int32.Parse(codesQty);
                    if (qty < 1 || qty > 1000)
                    {
                        check = false;
                        MessageBox.Show("Допустимое количество кодов от 1 до 1000 (по умолчанию 1).");
                    }
                }
                catch
                {
                    check = false;
                    MessageBox.Show("В поле \"Количество кодов\" должны быть только цифры.");
                }
            }

            if (codesLength != "")
            {
                try
                {
                    length = Int32.Parse(codesLength);
                    if (length < 10 || length > 20)
                    {
                        check = false;
                        MessageBox.Show("Допустимая длина кода от 10 до 20 символов(по-умолчанию 10).");
                    }
                }
                catch
                {
                    check = false;
                    MessageBox.Show("В поле \"Длинна кода\" должны быть только цифры.");
                }
            }
            return check;
        }
        private string GenerateCode(int length)
        {
            char[] codeSymbols = new char[]
                {
                    '1', '2', '3', '4', '5','6','7','8','9',
                    'Q','W','E','R','T','Y','U','I','P','A','S','D','F','G','H','J','K','L','Z','X','C','V','B','N','M',
                    'q','w','e','r','t','y','u','i','p','a','s','d','f','g','h','j','k','l','z','x','c','v','b','n','m',
                };
            string result   = string.Empty;
            char[] temp     = new char[length];
            int rndResult;

            for (int i = 0; i < length; i++)
            {
                rndResult   = rnd.Next(codeSymbols.Length);
                temp[i]     = codeSymbols[rndResult];
            }
            result = result + new String(temp);
            return result;
        }
    }
}
