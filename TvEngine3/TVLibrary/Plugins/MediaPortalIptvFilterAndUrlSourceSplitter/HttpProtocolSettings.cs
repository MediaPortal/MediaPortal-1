using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for HTTP protocol settings.
    /// </summary>
    [XmlRoot("HttpProtocolSettings")]
    public class HttpProtocolSettings : ProtocolSettings
    {
        #region Private fields

        private int openConnectionTimeout;
        private int openConnectionSleepTime;
        private int totalReopenConnectionTimeout;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="HttpProtocolSettings"/> class.
        /// </summary>
        public HttpProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = HttpProtocolSettings.DefaultHttpOpenConnectionTimeout;
            this.OpenConnectionSleepTime = HttpProtocolSettings.DefaultHttpOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = HttpProtocolSettings.DefaultHttpTotalReopenConnectionTimeout;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timeout to open HTTP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        public int OpenConnectionTimeout
        {
            get { return this.openConnectionTimeout; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("OpenConnectionTimeout", value, "Cannot be less than zero.");
                }

                this.openConnectionTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep before opening connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionSleepTime"/> is lower than zero.</para>
        /// </exception>
        public int OpenConnectionSleepTime
        {
            get { return this.openConnectionSleepTime; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("OpenConnectionSleepTime", value, "Cannot be less than zero.");
                }

                this.openConnectionSleepTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the total timeout to open HTTP url in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="TotalReopenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        public int TotalReopenConnectionTimeout
        {
            get { return this.totalReopenConnectionTimeout; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("TotalReopenConnectionTimeout", value, "Cannot be less than zero.");
                }

                this.totalReopenConnectionTimeout = value;
            }
        }

        #endregion

        #region Methods
        #endregion

        #region Constants

        public static readonly int DefaultHttpOpenConnectionTimeout = 20000;           // ms
        public static readonly int DefaultHttpOpenConnectionSleepTime = 0;             // ms
        public static readonly int DefaultHttpTotalReopenConnectionTimeout = 60000;    // ms

        #endregion
    }
}
