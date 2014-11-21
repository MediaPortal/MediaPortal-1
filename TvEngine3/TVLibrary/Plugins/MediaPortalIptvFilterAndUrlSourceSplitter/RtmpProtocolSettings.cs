using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for RTMP protocol settings.
    /// </summary>
    [XmlRoot("RtmpProtocolSettings")]
    public class RtmpProtocolSettings : ProtocolSettings
    {
        #region Private fields

        private int openConnectionTimeout;
        private int openConnectionSleepTime;
        private int totalReopenConnectionTimeout;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpProtocolSettings"/> class.
        /// </summary>
        public RtmpProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = RtmpProtocolSettings.DefaultRtmpOpenConnectionTimeout;
            this.OpenConnectionSleepTime = RtmpProtocolSettings.DefaultRtmpOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = RtmpProtocolSettings.DefaultRtmpTotalReopenConnectionTimeout;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timeout to open RTMP url in milliseconds.
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
        /// Gets or sets the total timeout to open RTMP url in milliseconds.
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

        public static readonly int DefaultRtmpOpenConnectionTimeout = 20000;           // ms
        public static readonly int DefaultRtmpOpenConnectionSleepTime = 0;             // ms
        public static readonly int DefaultRtmpTotalReopenConnectionTimeout = 60000;    // ms

        #endregion
    }
}
