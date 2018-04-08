using System;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NicheNameJacker.Models;
using NicheNameJacker.Utilities;

namespace NicheNameJacker.Common.Membership
    {
    class LicenseEngineManager
        {

        #region Singleton Instance


        private LicenseEngineManager ()
            {
            }

        private static readonly object padlock = new object();

        private static LicenseEngineManager instance = null;
        public static LicenseEngineManager Instance
            {
            get
                {
                lock ( padlock )
                    {
                    if ( instance == null )
                        {
                        instance = new LicenseEngineManager();
                        }
                    return instance;
                    }
                }
            }


        #endregion Singleton Instance

        #region Methods


        /// <summary>
        /// Method to send check license status from license engine.
        /// </summary>
        /// <param name="key">License key string.</param>
        /// <returns></returns>
        public async Task<LicenseItem> ShowLicenseStats (string key)
            {
            LicenseItem result = null;

            try
                {
                HttpClient httpClient = new HttpClient();

                string url = string.Format(Properties.Constants.LicenseEngineShowLicenseStatsFormat,
                                           key);

                HttpResponseMessage response = await httpClient.GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();

                if ( string.IsNullOrEmpty(jsonString) == false )
                    {
                    result = JsonConvert.DeserializeObject<LicenseItem>(jsonString);
                    }
                }
            catch ( Exception ex )
                {
                Logger.LogError($"Exception occured while check license in license engine. {ex.Message}");
                }

            return result;
            }

        /// <summary>
        /// Method to send get request to license engine.
        /// </summary>
        /// <param name="key">License key string.</param>
        /// <returns></returns>
        public async Task<LicenseItem> CheckLicense (string key)
            {
            LicenseItem result = null;

            try
                {
                HttpClient httpClient = new HttpClient();

                string url = string.Format(Properties.Constants.LicenseEngineCheckLicenseFormat,
                                           key,
                                           GetHDDSerial());

                HttpResponseMessage response = await httpClient.GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();

                if ( string.IsNullOrEmpty(jsonString) == false )
                    {
                    result = JsonConvert.DeserializeObject<LicenseItem>(jsonString);
                    }
                }
            catch ( Exception ex )
                {
                Logger.LogError($"Exception occured while check license in license engine. {ex.Message}");
                }

            return result;
            }

        /// <summary>
        /// Method to get license stats from license engine.
        /// </summary>
        /// <param name="key">License key string.</param>
        /// <returns></returns>
        public async Task<LicenseStats> LicenseStats (string key)
            {
            LicenseStats result = null;

            try
                {
                HttpClient httpClient = new HttpClient();

                string url = string.Format(Properties.Constants.LicenseEngineShowLicenseStatsFormat,
                                           key);

                httpClient.DefaultRequestHeaders
                                                .Accept
                                                .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                HttpResponseMessage response = await httpClient.GetAsync(url);
                string jsonString = await response.Content.ReadAsStringAsync();

                if ( string.IsNullOrEmpty(jsonString) == false )
                    {
                    var root = JsonConvert.DeserializeObject<LicenseStatsRoot>(jsonString);
                    if ( root != null && root.data != null )
                        {
                        result = root.data;
                        }
                    }
                }
            catch ( Exception ex )
                {
                Logger.LogError($"Exception occured while license stats in license engine. {ex.Message}");
                }

            return result;
            }

        /// <summary>
        /// Method to get HDD serial.
        /// Reference: https://stackoverflow.com/a/7244444/2077741
        /// https://www.codeproject.com/Articles/6077/How-to-Retrieve-the-REAL-Hard-Drive-Serial-Number
        /// </summary>
        /// <returns></returns>
        private string GetHDDSerial ()
            {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            ManagementObjectCollection managementObjects = searcher.Get();

            foreach ( ManagementObject wmi_HD in managementObjects )
                {
                // get the hardware serial no.
                if ( wmi_HD["SerialNumber"] != null )
                    {
                    return wmi_HD["SerialNumber"].ToString();
                    }
                }

            return "None";
            }


        #endregion Methods

        }
    }
