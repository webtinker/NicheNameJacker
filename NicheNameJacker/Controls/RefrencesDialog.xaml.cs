using NicheNameJacker.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NicheNameJacker.Extensions;
using NicheNameJacker.Properties;
using System.Threading.Tasks;

namespace NicheNameJacker.Controls
{
    public partial class RefrencesDialog : Window, IDisposable
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public RefrencesViewModel ViewModel { get; }

        public RefrencesDialog(string address)
        {
            DataContext = ViewModel = new RefrencesViewModel(address, _cts.Token);

            InitializeComponent();
            Subtitle.Text += " " + address;

            Loaded += RefrencesDialog_Loaded;

            Closed += (s, e) => Dispose();
        }

        private async void RefrencesDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(YoutubeListView.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("SearchResult.ViewCount", ListSortDirection.Descending));


            await Task.Delay(2000);
            Notification.PopupVisibility = Visibility.Visible;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            switch (TabControl.SelectedIndex)
            {
                case 0:
                    ViewModel.IsSearchEnabled = !Settings.Default.YouTubeApiKey.IsNullOrEmpty() && !Settings.Default.YouTubeClientId.IsNullOrEmpty();
                    if (ViewModel.IsYouTubeReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.YouTubeRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                case 1:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsRedditReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.RedditRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                case 2:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsTumblrReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.TumblrRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                case 3:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsWikipediaReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.WikipediaRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                case 4:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsHuffpostReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.HuffpostRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                case 5:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsMashableReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.MashableRefrences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;

                case 6:
                    ViewModel.IsSearchEnabled = true;
                    if (ViewModel.IsYahooReferencesAvailable)
                    {
                        ViewModel.IsBackLinkCountVisible = true;
                        ViewModel.BackLinkCount = ViewModel.YahooReferences.Count.ToString("N0");
                    }
                    else
                    {
                        ViewModel.IsBackLinkCountVisible = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();

            Dispose();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
