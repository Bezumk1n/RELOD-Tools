using System;
using System.Windows;

namespace RELOD_Tools
{
    /// <summary>
    /// Логика взаимодействия для ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : Window
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        public void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
