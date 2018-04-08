using System;
using System.Windows;
using System.Windows.Controls;
using NicheNameJacker.ViewModels;
using NicheNameJacker.Controls;
using NicheNameJacker.Utilities;
using Microsoft.Win32;
using NicheNameJacker.Common;
using System.Reflection;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Diagnostics;

namespace Application
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            //configuring log4net to read settings from app.config
            log4net.Config.XmlConfigurator.Configure();
            DataContext = ViewModel = new MainViewModel();
            var vm = this.DataContext as MainViewModel;
            if (vm != null)
            {
                // messagebox
                var popup = (Action)(()=>
                {
                    var window = new CompleteDialog();
                    window.Owner = this;
                    window.ShowDialog();
                });

                vm.SetupCompleteDialog(popup);
            }
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //InitializeBrowser(StartPageBrowser, Urls.StartPage);
            //InitializeBrowser(ExpyredBrowser, Urls.Expyred);
            //InitializeBrowser(NicheReaperBrowser, Urls.NicheReaper, true);
            //InitializeBrowser(ContentReaperBrowser, Urls.ContentReaper, true);
            //InitializeBrowser(WeirdBrowser, Urls.Weird, true);
            //InitializeBrowser(VideoMakerBrowser, Urls.VideoMaker, true);
            //InitializeBrowser(AutoBlogBrowser, Urls.AutoBlog, true);
            //InitializeBrowser(WebBrowserLeft, Urls.LeftAds);

            InitializeFiltering();

            ViewModel.OnStartAsync().ConfigureAwait(false);
            ViewModel.Pbn.LoadDataAsync().ConfigureAwait(false);

            Logger.LogStatuses($"OS version: {Environment.OSVersion}", $"Is 64 bit: {Environment.Is64BitOperatingSystem}", "App successfully started");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //CreditsUsedManager.SaveCredits();
            base.OnClosing(e);
        }

        static void InitializeBrowser(WebBrowser browser, string url, bool supressJavascriptErrors = false)
        {
            if (supressJavascriptErrors)
            {
                browser.Loaded += (s, a) => HideScriptErrors(s);
                browser.Navigated += (s, a) =>
                {
                    if (browser.IsLoaded)
                    {
                        HideScriptErrors(s);
                    }
                };
            }

            if (browser.IsVisible)
            {
                browser.Navigate(url);
            }
            else
            {
                DependencyPropertyChangedEventHandler handler = null;
                handler = (s, e) =>
                {
                    browser.IsVisibleChanged -= handler;
                    browser.Navigate(url);
                };
                browser.IsVisibleChanged += handler;
            }
        }

        static void HideScriptErrors(object browser, bool hide = true)
        {
            try
            {
                var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return;
                var objComWebBrowser = fiComWebBrowser.GetValue(browser);
                if (objComWebBrowser == null) return;
                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
            }
            catch (Exception)
            {
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) => new SettingsDialog().ShowDialog();

        private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutDialog().ShowDialog();

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var listView = (ListView)button.CommandParameter;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            var sortDirection = ListSortDirection.Ascending;
            if (view.SortDescriptions.Any() && view.SortDescriptions.First().Direction == ListSortDirection.Ascending)
            {
                sortDirection = ListSortDirection.Descending;
            }
            view.SortDescriptions.Clear();
            view.CustomSort = null;
            view.SortDescriptions.Add(new SortDescription(nameof(SingleDomain.Address), sortDirection));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
            SetKeyforWebBrowserControl(appName);
        }

        /// <summary>
        /// This helper function forces the internal web browser control
        /// for the application to IE9. 
        /// </summary>
        /// <param name="appName"></param>
        private void SetKeyforWebBrowserControl(string appName)
        {
            RegistryKey regkey = null;
            try
            {
                //For 64 bit Machine 
                if (Environment.Is64BitOperatingSystem)
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                else  //For 32 bit Machine 
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);

                //If the path is not correct or 
                //If user't have priviledges to access registry 
                if (regkey == null)
                {
                    return;
                }

                string findAppkey = Convert.ToString(regkey.GetValue(appName));

                //Check if key is already present 
                if (findAppkey == @"9999")
                {
                    regkey.Close();
                    return;
                }

                //If key is not present add the key , Kev value 8000-Decimal 
                if (string.IsNullOrEmpty(findAppkey))
                    regkey.SetValue(appName, unchecked((int)0x270F), RegistryValueKind.DWord);

                //check for the key after adding 
                /*
                findAppkey = Convert.ToString(regkey.GetValue(appName));

                if (findAppkey == @"9999")
                    MessageBox.Show(@"Application Settings Applied Successfully");
                else
                    MessageBox.Show(@"Application Settings Failed, Ref: " + findAppkey);
                */

            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not force browser to IE9: {ex.Message}");
            }
            finally
            {
                //Close the Registry
                regkey?.Close();
            }


        }

        private void BlacklistSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var listview = (ListView)textbox.Tag;

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(h => textbox.TextChanged += h, h => textbox.TextChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOnDispatcher()
                .Subscribe(_ => listview.FilterBy<SingleDomain>(d => d.Address.Contains(textbox.Text)));
        }

        private void Web2BlacklistSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var listview = (DataGrid)textbox.Tag;

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(h => textbox.TextChanged += h, h => textbox.TextChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOnDispatcher()
                .Subscribe(_ => listview.FilterBy<SingleDomain>(d => d.Address.Contains(textbox.Text)));
        }

        private void BlockedDomainAddressFilter_Loaded(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            var dataGrid = (DataGrid)textbox.Tag;

            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(h => textbox.TextChanged += h, h => textbox.TextChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOnDispatcher()
                .Subscribe(_ => dataGrid.FilterBy<SingleDomain>(d => d.Address.Contains(textbox.Text)));
        }

        private void InitializeFiltering()
        {
            var checkboxes = new[] { ContainsCheck, StartsWithCheck, EndsWithCheck, NoNumbersCheck, NoHyphensCheck, TopLevelCheck };

            var checkBoxObservable = checkboxes.Select(c => new
            {
                Checked = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => c.Checked += h, h => c.Checked -= h),
                UnChecked = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => c.Unchecked += h, h => c.Unchecked -= h)
            })
            .Select(x => x.Checked.Merge(x.UnChecked))
            .Aggregate((x, y) => x.Merge(y));

            var textBoxObservable = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(h => ExtensionFilterBox.TextChanged += h, h => ExtensionFilterBox.TextChanged -= h);

            var finalObservable = checkBoxObservable.MergeUnits(textBoxObservable);

            finalObservable.Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOnDispatcher()
                .Subscribe(_ => UpdateFilter());
        }

        private void UpdateFilter()
        {
            var list = new List<Func<SingleDomain, bool>>();

            if (StartsWithCheck.IsChecked == true)
            {
                list.Add(s => s.Address.StartsWith(SearchTextBox.Text));
            }

            if (EndsWithCheck.IsChecked == true)
            {
                list.Add(s => s.Address.EndsWith(SearchTextBox.Text));
            }

            if (ContainsCheck.IsChecked == true)
            {
                list.Add(s => s.Address.Contains(SearchTextBox.Text));
            }

            if (NoNumbersCheck.IsChecked == true)
            {
                list.Add(s => s.Address.None(char.IsNumber));
            }

            if (NoHyphensCheck.IsChecked == true)
            {
                list.Add(s => !s.Address.Contains('-'));
            }

            if (TopLevelCheck.IsChecked == true)
            {
                list.Add(s => !s.DomainName.Contains('.'));
            }

            if (!string.IsNullOrWhiteSpace(ExtensionFilterBox.Text))
            {
                var extensions = ExtensionFilterBox.Text.Split(',').Select(s => s.Trim());
                list.Add(s => extensions.Any(d => s.Address.EndsWith(d)));
            }

            //DomainItemsView.FilterBy<SingleDomain>(d => list.All(f => f(d)));
        }

        private void BackButtonClick(object sender, RoutedEventArgs e) => NavigateBrowser(sender as Button, false);

        private void ForwardButtonClick(object sender, RoutedEventArgs e) => NavigateBrowser(sender as Button, true);

        private void NavigateBrowser(Button button, bool forward, WebBrowser browser = null)
        {
            browser = browser ?? button.CommandParameter as WebBrowser;
            var canNavigate = forward ? browser.CanGoForward : browser.CanGoBack;

            if (canNavigate)
            {
                button.IsEnabled = false;

                NavigatedEventHandler navigated = null;
                navigated = (s, a) =>
                {
                    browser.Navigated -= navigated;
                    button.IsEnabled = true;
                };
                browser.Navigated += navigated;

                if (forward)
                {
                    browser.GoForward();
                }
                else
                {
                    browser.GoBack();
                }
            }
        }

        private void OrginzePbnFavorites_Click(object sender, RoutedEventArgs e)
        {
            //PbnFavoritesListView.SortBy<SingleDomain, string>(x => x.DomainName.Invert());
        }

        private void ViewArchive_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var address = button.CommandParameter as string;
            Process.Start(Urls.ArhivePrefix + address);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var listView = button.CommandParameter as ListView;
            listView.SelectAll();
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var listView = button.CommandParameter as ListView;
            var selected = listView.SelectedItems;
            listView.UnselectAll();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FavoriteItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
