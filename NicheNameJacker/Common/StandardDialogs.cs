using System.Windows.Forms;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace NicheNameJacker.Common
{
    public static class StandardDialogs
    {
        public static string RequestFolderPath()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.SelectedPath : null;
        }

        public static string RequestCsvSaveFilename() => RequestSaveFileName("Domains", ".csv", "CSV Files (.csv)|*.csv");

        public static string RequestTxtSaveFilename(string filename) => RequestSaveFileName(filename, ".txt", "Text Files (.txt)|*.txt");

        public static string RequestSaveFileName(string filename, string extension, string filter)
        {
            var dialog = new SaveFileDialog
            {
                FileName = filename,
                DefaultExt = extension,
                Filter = filter
            };
            var result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }

        public static string OpenFile(string extension = "txt")
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"(.{extension})|*.{extension}"
            };
            var result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.FileName : null;
        }
    }
}
