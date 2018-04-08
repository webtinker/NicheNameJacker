using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using YoutubeExtractor;
using MessageBox = System.Windows.MessageBox;

namespace NicheNameJacker.Schema
{
    public sealed class YouTubeSearchResult : BaseSearchResult
    {
        public YouTubeSearchResult() : base() { }

        public string ChannelTitle { get; set; }

        public ulong? DislikeCount { get; internal set; }
        public string DislikeCountString => DislikeCount.HasValue ? $"{DislikeCount} dislikes" : "";

        public ulong? LikeCount { get; set; }
        public string LikeCountString => LikeCount.HasValue ? $"{LikeCount} likes" : "";

        public DateTime? PublishedDate { get; set; }

        public ulong? ViewCount { get; set; }
        public string ViewCountString => ViewCount.HasValue ? $"{ViewCount} views" : "";

        public string ThumbnailUrl { get; set; }

        public string Duration { get; set; }
        public string DurationString => $"duration {Duration}";

        //public string EmbedCode => 
        //    $@"<iframe src=""http://www.youtube.com/embed/{Id}"" frameborder=""0"" allowfullscreen></iframe>";

        private bool isDownloading;
        public bool IsDownloading
        {
            get { return isDownloading; }
            set { SetProperty(ref isDownloading, value); }
        }

        private double progress;
        public double Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }

        public ICommand DownloadCommand => new RelayCommand(async () =>
        {
            Progress = 0;
            IsDownloading = false;
            var folderPath = StandardDialogs.RequestFolderPath();
            if (folderPath != null)
            {
                await Download(folderPath);
            }
        });

        async Task Download(string destinationFolder)
        {
            Progress = 0;
            IsDownloading = true;
            try
            {
                await Task.Run(() =>
                {
                    var videoInfos = DownloadUrlResolver.GetDownloadUrls(SourceAddress);

                    var video = videoInfos.FirstOrDefault(info => info.VideoType == VideoType.Mp4);

                    if (video.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(video);
                    }

                    var invalidChars = Path.GetInvalidFileNameChars();
                    var fileName = (video.Title + video.VideoExtension).Where(c => !invalidChars.Contains(c)).JoinStrings();
                    var videoDownloader = new VideoDownloader(video, Path.Combine(destinationFolder, fileName));
                    videoDownloader.DownloadProgressChanged += (sender, args) => Progress = args.ProgressPercentage;

                    Observable.FromEventPattern<EventHandler<ProgressEventArgs>, ProgressEventArgs>
                        (h => videoDownloader.DownloadProgressChanged += h, h => videoDownloader.DownloadProgressChanged -= h)
                        .Throttle(TimeSpan.FromMilliseconds(100))
                        .ObserveOn(App.Current.Dispatcher)
                        .Subscribe(a => Progress = a.EventArgs.ProgressPercentage);

                    videoDownloader.Execute();
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("Download failed!");
            }
        }
    }
}
