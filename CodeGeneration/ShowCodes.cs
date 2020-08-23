using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace RELOD_Tools.CodeGeneration
{
    public class ShowCodes
    {
        public ShowCodes(string dbName, string dbPath)
        {
            List<CodeModel> codesList = new List<CodeModel>();
            SQLiteConnection connection = new SQLiteConnection("Data Source = " + dbPath + dbName);
            connection.Open();
            SQLiteCommand cmd = new SQLiteCommand(connection);

            cmd.CommandText = "SELECT * FROM codes";
            SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                codesList.Add(new CodeModel { Id = rdr.GetInt32(0), Code = rdr.GetString(1), Date = rdr.GetString(2) });
            }

            rdr.Close();
            connection.Close();

            DataGrid DG = new DataGrid();
            DG.Title = "Коды";
            DG.Show();
            DG.dataGrid.ItemsSource = codesList;
        }
    }
}
