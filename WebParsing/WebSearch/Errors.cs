using System;
using System.Windows;

namespace RELOD_Tools.WebParsing.WebSearch
{
    static class Errors
    {
        public static void LoginPageError(Exception ex)
        {
            MessageBox.Show("При попытке залогиниться произошла ошибка :(\n" + ex, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void CustomError(Exception ex)
        {
            MessageBox.Show("При выполнении программы что-то пошло не так :(\n" + ex, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void EmptyISBNError()
        {
            MessageBox.Show("Введите хотя бы один ISBN.");
        }
    }
}
