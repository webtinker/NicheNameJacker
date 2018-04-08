using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NicheNameJacker.Common
{
    class KeywordsProvider
    {
        public static string[] GetFromFile()
        {
            string fileName = StandardDialogs.OpenFile();
            if (Path.GetExtension(fileName) != ".txt")
            {
                MessageBox.Show($@"Only files with .txt extension are available", "Alert", MessageBoxButton.OK);
                return null;
            }

            if (!File.Exists(fileName))
            {
                MessageBox.Show($@"File {fileName} doesn't exist", "Alert", MessageBoxButton.OK);
                return null;
            }

            string contents = File.ReadAllText(fileName);
            string[] keywords = contents.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (keywords.Length > 50)
            {
                MessageBox.Show($@"You can't import more than 50 keywords", "Alert", MessageBoxButton.OK);
                return null;
            }

            return keywords;
        }
    }
}
