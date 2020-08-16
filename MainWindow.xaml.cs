using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RELOD_Tools.PriceList;
using RELOD_Tools.Logic;
using RELOD_Tools.CodeGeneration;
using RELOD_Tools.WebSearch;
using RELOD_Tools.WebParsing.WebSearch;
using RELOD_Tools.AuthorsCompare;

namespace RELOD_Tools
{
    public partial class MainWindow : Window
    {
        string exceptionsDirectory  = @".\PriceList";
        string exceptionsFilePath   = @".\PriceList\Exceptions.txt";

        string dbName               = "database.db";
        string dbPath               = @".\";

        public MainWindow()
        {
            InitializeComponent();

            // Для вкладки Прайс-лист подгружаем из файла группы для исключения (если нет папки или файла - создаем)
            groupsForExclude.Text = WorkWithFile.CheckForExceptionsFileExistance(
                exceptionsDirectory,
                exceptionsFilePath
                );

            // На вкладке Прайс-лист выключаем кнопку Сохранить (включается, если будут изменения в поле с группами для исключений)
            saveGroupsBtn.IsEnabled = false;

            // Проверяем, есть ли файл базы данных (если нет то создаем). База используется только для хранения сгенерированных кодов
            WorkWithFile.CreateDataBase(
                dbName,
                dbPath
                );            
        }
        
        // Блок поиска по сайтам
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (userInput.Text != "")
            {
                string[] isbns = userInput.Text.Split('\n');
                userInput.Clear();

                this.Hide();
                SelectSite.Search(
                    isbns,
                    webSite.Text
                    );
                this.Show();
            }
            else
            {
                Errors.EmptyISBNError();
            }
        }
        // =============================================
        
        // Блок создания прайс-листа
        private void GeneratePrice_Click(object sender, RoutedEventArgs e)
        {
            new DoPriceList(groupsForExclude.Text, fullPrice.IsChecked);
        }
        private void SaveGroup_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(
                exceptionsFilePath,
                groupsForExclude.Text,
                Encoding.UTF8
                );
            saveGroupsBtn.IsEnabled = false;
        }

        // Функция, включающая кнопку "Сохранить" если в поле были какие-либо изменения
        private void GroupsForExclude_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveGroupsBtn.IsEnabled = true;
        }
        // =============================================

        // Блок генерирования кодов
        private void GenerateCode_Click(object sender, RoutedEventArgs e)
        {
            new CodeGen(
                codesQTY.Text,
                codesLength.Text,
                mustStartWith.Text,
                mustEnd.Text,
                dbName,
                dbPath
                );
        }
        private void ShowCodes_Click(object sender, RoutedEventArgs e)
        {
            new ShowCodes(
                dbName,
                dbPath
                );
        }
        // =============================================
        
        // Блок сравнения авторов
        private void CompareAuthors_Click(object sender, RoutedEventArgs e)
        {
            new AuthorsComparer(authors.Text);
            authors.Clear();
        }
        // =============================================
    }
}
