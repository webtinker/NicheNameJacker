namespace NicheNameJacker.Models
    {
    /// <summary>
    /// Class used to represent License Engine stats.
    /// </summary>
    public class LicenseStats
        {

        #region Properties


        /// <summary>
        /// String representing license id.
        /// </summary>
        public int id
            {
            get; set;
            }

        /// <summary>
        /// String representing license plugin id.
        /// </summary>
        public int plugin_id
            {
            get; set;
            }

        /// <summary>
        /// String representing license product id.
        /// </summary>
        public string product_id
            {
            get; set;
            }

        /// <summary>
        /// String representing license name.
        /// </summary>
        public string name
            {
            get; set;
            }

        /// <summary>
        /// String representing license email.
        /// </summary>
        public string email
            {
            get; set;
            }

        /// <summary>
        /// String representing license key.
        /// </summary>
        public string license_key
            {
            get; set;
            }

        /// <summary>
        /// String representing license expires.
        /// </summary>
        public string expires
            {
            get; set;
            }

        /// <summary>
        /// String representing license active.
        /// </summary>
        public int active
            {
            get; set;
            }

        /// <summary>
        /// String representing license reason.
        /// </summary>
        public string reason
            {
            get; set;
            }

        /// <summary>
        /// String representing license notes.
        /// </summary>
        public string notes
            {
            get; set;
            }

        /// <summary>
        /// String representing license domain limit.
        /// </summary>
        public int domain_limit
            {
            get; set;
            }

        /// <summary>
        /// String representing license created at.
        /// </summary>
        public string created_at
            {
            get; set;
            }

        /// <summary>
        /// String representing license updated at.
        /// </summary>
        public string updated_at
            {
            get; set;
            }


        #endregion Properties

        }

    /// <summary>
    /// Class used to represent License Engine stats root.
    /// </summary>
    public class LicenseStatsRoot
        {

        #region Properties


        /// <summary>
        /// String representing license stats data.
        /// </summary>
        public LicenseStats data
            {
            get; set;
            }


        #endregion Properties

        }
    }
