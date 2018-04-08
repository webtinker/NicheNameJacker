using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NicheNameJacker.Common.Membership;
using NicheNameJacker.Properties;
using System.Windows;
using System.Windows.Navigation;
using NicheNameJacker.Extensions;

namespace NicheNameJacker.Controls
{
    public partial class SettingsDialog : Window
    {

        public SettingsDialog()
        {
            InitializeComponent();
            //DomDetalierKeyTextBox.Password = Settings.Default.DomDetailerKey;
            YouTubeApiKeyTextBox.Text = Settings.Default.YouTubeApiKey;
            YouTubeClientIdTextBox.Text = Settings.Default.YouTubeClientId;
            SetupUICheckboxes();
            
        }

        private void SetupUICheckboxes()
        {
            if (PermissionAssistant.CurrentPermissions.MembershipPlan == MembershipPlan.Elite ||
                    PermissionAssistant.CurrentPermissions.MembershipPlan == MembershipPlan.Trial ||
                    PermissionAssistant.CurrentPermissions.MembershipPlan == MembershipPlan.Lifetime)
            {
                AutoOptionPanel.Visibility = Visibility.Visible;
                AutoDeletePanel.Visibility = Visibility.Visible;
                AutoMetrics.IsChecked = Settings.Default.AutoStats;
                AutoDel.IsChecked = Settings.Default.AutoDel;

            }
            else
            {
                AutoDeletePanel.Visibility = Visibility.Collapsed;
                AutoOptionPanel.Visibility = Visibility.Collapsed;
                AutoMetrics.IsChecked = false;
                AutoDel.IsChecked = false;
                this.MaxHeight = 560;
            }
        }

        public bool ShowAutoOption {
            get
            {
                if (PermissionAssistant.CurrentPermissions.MembershipPlan == MembershipPlan.Elite ||
                    PermissionAssistant.CurrentPermissions.MembershipPlan == MembershipPlan.Trial)
                {
                    return true;
                }
                return false;
            }

        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //Settings.Default.DomDetailerKey = DomDetalierKeyTextBox.Password;
            Settings.Default.YouTubeApiKey = YouTubeApiKeyTextBox.Text;
            Settings.Default.YouTubeClientId = YouTubeClientIdTextBox.Text;
            Settings.Default.AutoStats = AutoMetrics.IsChecked.HasValue ? AutoMetrics.IsChecked.Value : false;
            Settings.Default.AutoDel = AutoDel.IsChecked.HasValue ? AutoDel.IsChecked.Value : false;
            Settings.Default.Save();
            PermissionAssistant.InvalidateYouTubeSettingsState();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            VerificationStatusText.Text = "";
            if (string.IsNullOrEmpty(RegistrationKeyBox.Password)) return;

            AccountProgressBar.Visibility = Visibility.Visible;
            VerifyButton.IsEnabled = false;

            var license = await LicenseEngineManager.Instance.CheckLicense(RegistrationKeyBox.Password);
            if ( license != null && license.license.Equals("valid") )
                {
                PermissionAssistant.SetMembershipPlan(MembershipPlan.Lifetime);
                if ( PermissionAssistant.CurrentPermissions.MembershipPlan != MembershipPlan.Lifetime )
                    {
                    PermissionAssistant.SetMembershipPlan(MembershipPlan.Lifetime);
                    Settings.Default.RegistrationKey = RegistrationKeyBox.Password;
                    Settings.Default.Save();
                    }
                VerificationStatusText.Text = $"Your membeship plan is updated to {MembershipPlan.Lifetime}";
                }
            else
                {
                VerificationStatusText.Text = "The Registration Key you provided couldn't be verified";
                }

            //var plan = await PermissionAssistant.VerifyKey(RegistrationKeyBox.Password);
            //if (plan == null)
            //{
            //    VerificationStatusText.Text = "The Registration Key you provided couldn't be verified";
            //}
            //else
            //{
            //    if (PermissionAssistant.CurrentPermissions.MembershipPlan != plan.Value)
            //    {
            //        PermissionAssistant.SetMembershipPlan(plan.Value);
            //        Settings.Default.RegistrationKey = RegistrationKeyBox.Password;
            //        Settings.Default.Save();
            //    }
            //    VerificationStatusText.Text = $"Your membeship plan is updated to {plan.Value}";
            //}

            AccountProgressBar.Visibility = Visibility.Hidden;
            VerifyButton.IsEnabled = true;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets", "Docs", "tutorial.pdf");
                Process.Start(new ProcessStartInfo(path));
            }
            catch (Exception)
            {
            }
            
            e.Handled = true;
        }

        private void AutoMetrics_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void AutoMetrics_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}
