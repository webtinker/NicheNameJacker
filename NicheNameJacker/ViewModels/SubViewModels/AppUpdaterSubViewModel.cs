using Newtonsoft.Json;
using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static NicheNameJacker.Common.Assistant;

namespace NicheNameJacker.ViewModels.SubViewModels
{
    public class AppUpdaterSubViewModel : ObservableBase
    {
        private string _downloadUrl;

        bool _updaterVisible;
        public bool UpdaterVisible { get { return _updaterVisible; } set { SetProperty(ref _updaterVisible, value); } }

        bool _downloadActive;
        public bool DownloadActive { get { return _downloadActive; } set { SetProperty(ref _downloadActive, value); } }

        public async Task CheckForTheNewVersion()
        {
            _downloadUrl = await GetNewVersionDownloadUrl();
            if (_downloadUrl != null)
            {
                UpdaterVisible = true;
            }
        }

        async Task<string> GetNewVersionDownloadUrl()
        {
            try
            {
                var url = "http://nichenamejacker.com/updates/version.json";
                var jsonString = await HttpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeAnonymousType(jsonString, new { version = "", downloadUrl = "" });
                return data.version != AssemblyInfo.VersionNumber ? data.downloadUrl : null;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }

        async Task<bool> DownloadNewVersion(string url, string destination)
        {
            try
            {
                var bytes = await HttpClient.GetByteArrayAsync(url);
                using (var fs = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    fs.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                return false;
            }
        }

        public ICommand HideCommand => new RelayCommand(() => UpdaterVisible = false);

        public ICommand DownloadCommand => new RelayCommand(async () =>
        {
            var folder = StandardDialogs.RequestFolderPath();
            if (folder != null)
            {
                DownloadActive = true;

                var path = Path.Combine(folder, "Setup.exe");
                var result = await DownloadNewVersion(_downloadUrl, path);
                if (result && File.Exists(path))
                {
                    Process.Start(path);
                }
                else
                {
                    MessageBox.Show("Update attempt was unsuccessful");
                }
                DownloadActive = UpdaterVisible = false;
            }
        });
    }
}
