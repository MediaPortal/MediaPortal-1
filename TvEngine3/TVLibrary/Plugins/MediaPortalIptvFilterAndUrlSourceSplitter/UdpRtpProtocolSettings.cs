using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for UDP or RTP protocol settings.
    /// </summary>
    [XmlRoot("UdpRtpProtocolSettings")]
    public class UdpRtpProtocolSettings : ProtocolSettings
    {
        #region Private fields

        private int receiveDataCheckInterval;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UdpRtpProtocolSettings"/> class.
        /// </summary>
        public UdpRtpProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = UdpRtpUrl.DefaultUdpOpenConnectionTimeout;
            this.OpenConnectionSleepTime = UdpRtpUrl.DefaultUdpOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = UdpRtpUrl.DefaultUdpTotalReopenConnectionTimeout;

            this.ReceiveDataCheckInterval = UdpRtpUrl.DefaultUdpReceiveDataCheckInterval;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the receive data check interval in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ReceiveDataCheckInterval"/> is lower than zero.</para>
        /// </exception>
        public int ReceiveDataCheckInterval
        {
            get { return this.receiveDataCheckInterval; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ReceiveDataCheckInterval", value, "Cannot be less than zero.");
                }

                this.receiveDataCheckInterval = value;
            }
        }

        #endregion

        #region Methods
        #endregion
    }
}
