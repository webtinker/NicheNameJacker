using NicheNameJacker.ViewModels;
using System.Windows;

namespace NicheNameJacker.Controls
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            DataContext = new AboutViewModel();
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
