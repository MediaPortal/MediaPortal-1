using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for RTSP protocol settings.
    /// </summary>
    [XmlRoot("RtspProtocolSettings")]
    public class RtspProtocolSettings : ProtocolSettings
    {
        #region Private fields

        private int clientPortMin;
        private int clientPortMax;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtspProtocolSettings"/> class.
        /// </summary>
        public RtspProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = RtspUrl.DefaultRtspOpenConnectionTimeout;
            this.OpenConnectionSleepTime = RtspUrl.DefaultRtspOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = RtspUrl.DefaultRtspTotalReopenConnectionTimeout;

            this.ClientPortMin = RtspUrl.DefaultRtspClientPortMin;
            this.ClientPortMax = RtspUrl.DefaultRtspClientPortMax;
            this.SameConnectionPreference = RtspUrl.DefaultRtspSameConnectionTcpPreference;
            this.UdpPreference = RtspUrl.DefaultRtspUdpPreference;
            this.MulticastPreference = RtspUrl.DefaultRtspMulticastPreference;
            this.IgnoreRtpPayloadType = RtspUrl.DefaultRtspIgnoreRtpPayloadType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the minimum client port for UDP transport.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMin"/> is lower than zero.</para>
        /// <para>- or -</para>
        /// <para>The <see cref="ClientPortMin"/> is greater than 65535.</para>
        /// </exception>
        public int ClientPortMin
        {
            get { return this.clientPortMin; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be less than zero.");
                }

                if (value > 65535)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be greater than 65535.");
                }

                this.clientPortMin = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum client port for UDP transport.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMax"/> is lower than zero.</para>
        /// <para>- or -</para>
        /// <para>The <see cref="ClientPortMax"/> is greater than 65535.</para>
        /// </exception>
        public int ClientPortMax
        {
            get { return this.clientPortMax; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMax", value, "Cannot be less than zero.");
                }

                if (value > 65535)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMax", value, "Cannot be greater than 65535.");
                }

                this.clientPortMax = value;
            }
        }

        /// <summary>
        /// Gets or sets the preference of same connection.
        /// </summary>
        public int SameConnectionPreference { get; set; }

        /// <summary>
        /// Gets or sets the preference of UDP connection.
        /// </summary>
        public int UdpPreference { get; set; }

        /// <summary>
        /// Gets or sets the preference of UDP multicast connection.
        /// </summary>
        public int MulticastPreference { get; set; }

        /// <summary>
        /// Specifies if filter ignores check of received RTP packets payload type against payload type specified in SDP.
        /// </summary>
        public Boolean IgnoreRtpPayloadType { get; set; }

        #endregion

        #region Methods
        #endregion
    }
}
