namespace NicheNameJacker.Models
    {
    /// <summary>
    /// Class used to represent License Engine response.
    /// </summary>
    public class LicenseItem
        {

        #region Properties


        /// <summary>
        /// String representing license status.
        /// </summary>
        public string license
            {
            get; set;
            }

        /// <summary>
        /// String representing item name.
        /// </summary>
        public string item_name
            {
            get; set;
            }

        /// <summary>
        /// String representing additional message for user.
        /// </summary>
        public string message
            {
            get; set;
            }


        #endregion Properties

        }
    }
