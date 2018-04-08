using Application;
using System.Threading.Tasks;
using System.Windows;
using NicheNameJacker.Properties;
using System.Windows.Input;
using System;
using System.Linq;
using NicheNameJacker.Common.Membership;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NicheNameJacker.Common;
using NicheNameJacker.Utilities;
using log4net;

namespace NicheNameJacker.Controls
{
    public partial class AccountVerificationWindow : Window
    {
#if DEBUG
        private bool _isExperimental = Environment.GetCommandLineArgs()?.Contains("experimental") == true;
#else
        private bool _isExperimental;
#endif
        private readonly ILog _log;
       
        public AccountVerificationWindow()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); 
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
        }

        private async void AccountVerificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Assistant.UpgradeSettings();

            if (_isExperimental)
            {
                QuickAccessPanel.Visibility = Visibility.Visible;
                return;
            }

            if (!string.IsNullOrWhiteSpace(Settings.Default.RegistrationKey))
            {
                await VerifyRegistrationKey(Settings.Default.RegistrationKey);
            }
        }

        private async void RegistrationKeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !string.IsNullOrEmpty(RegistrationKeyBox.Password))
            {
                Keyboard.ClearFocus();
                await VerifyRegistrationKey(RegistrationKeyBox.Password);
            }
        }

        private void RegistrationKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            VerifyButton.Visibility = string.IsNullOrEmpty(RegistrationKeyBox.Password) ? Visibility.Collapsed : Visibility.Visible;
            VerificationStatusText.Visibility = Visibility.Collapsed;
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            await VerifyRegistrationKey(RegistrationKeyBox.Password);
        }

        private async Task VerifyRegistrationKey(string key)
        {
            MainGrid.Visibility = Visibility.Collapsed;
            ProgressPanel.Visibility = Visibility.Visible;
            _log.Info("Inside VerifyRegistrationKey");
            if (await IsKeyValid(key))
            {
                _log.Info("IsKeyValid returned true in VerifyRegistrationKey");

                if (key != Settings.Default.RegistrationKey)
                {
                    Settings.Default.RegistrationKey = key;
                    Settings.Default.Save();
                }
                SwitchToMainWindow();
            }
            else
            {
                VerificationStatusText.Visibility = Visibility.Visible;
            }

            MainGrid.Visibility = Visibility.Visible;
            ProgressPanel.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> IsKeyValid(string key)
        {
            _log.Info($"The Key Passed was : {key}");
            //var plan = await PermissionAssistant.VerifyKey(key);
            var license = await LicenseEngineManager.Instance.CheckLicense(key);

            _log.Info($"Plan name Returned : {license?.ToString()}");
            //if (plan != null)
            //{
            //    PermissionAssistant.SetMembershipPlan(plan.Value);
            //    _log.Info("Success setting plan");
            //    return true;
            //}
            if ( license != null && license.license.Equals("valid") )
                {
                PermissionAssistant.SetMembershipPlan(MembershipPlan.Lifetime);
                return true;
                }
            _log.Info("Plan was null");
            
            return false;
            }

        private void SwitchToMainWindow()
        {
            (App.Current.MainWindow = new MainWindow()).Show();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BypassAccountVerificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isExperimental)
            {
                var plan = (MembershipPlan)PlanCombo.SelectedValue;
                PermissionAssistant.SetMembershipPlan(plan);
                SwitchToMainWindow();
            }
        }

        private void MembershipLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(Urls.MembershipPage);
        }
    }
}
