using NicheNameJacker.Properties;
using NicheNameJacker.Utilities;
using ServiceStack.Text;
using System;
using System.IO;
using System.Net.Http;
using System.Xml;

namespace NicheNameJacker.Common
{
    public static class Assistant
    {
        static Assistant()
        {   // Configuring HttpClient to go and get result from the server each time rather than using the cache
            HttpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
        }
        public static TimeSpan? ToTimeSpan(string isoTime)
        {
            if (!string.IsNullOrWhiteSpace(isoTime))
            {

                try
                {
                    return XmlConvert.ToTimeSpan(isoTime);
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        private static Lazy<HttpClient> _httpClientCreator = new Lazy<HttpClient>(() => new HttpClient());
        /// <summary>
        /// Many HttpClient instance methods are pure.
        /// If we involve no changing state/mutation, it can safely be used in a parallel environment.
        /// </summary>
        public static HttpClient HttpClient => _httpClientCreator.Value;

        public static void UpgradeSettings()
        {
            if (!Settings.Default.SettingsAlreadyUpgraded)
            {
                Settings.Default.Upgrade();
                Settings.Default.SettingsAlreadyUpgraded = true;
                Settings.Default.Save();
            }
        }

        public static void SerializeToCsvFile(string filename, object data)
        {
            try
            {
                using (var writer = new StreamWriter(filename))
                {
                    CsvSerializer.SerializeToWriter(data, writer);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }
    }
}
