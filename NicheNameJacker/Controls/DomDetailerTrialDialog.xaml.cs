using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using NicheNameJacker.Common;
using NicheNameJacker.ViewModels;

namespace NicheNameJacker.Controls
{
    /// <summary>
    /// Interaction logic for DomDetailerTrialDialog.xaml
    /// </summary>
    public partial class DomDetailerTrialDialog : Window
    {
        public static bool WasDisplayed = false;

        public DomDetailerTrialDialog()
        {
            DataContext = new DomDetailerTrialViewModel();
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = !CreditsUsedManager.CreditsQuotaExhausted();
            Close();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(Urls.DomDetailerPage);
        }
    }
}
