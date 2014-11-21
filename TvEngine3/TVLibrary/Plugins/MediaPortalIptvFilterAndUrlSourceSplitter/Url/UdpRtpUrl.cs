using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represent base class for UDP or RTP urls for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class UdpRtpUrl : SimpleUrl
    {
        #region Private fields

        private int receiveDataCheckInterval = UdpRtpUrl.DefaultUdpReceiveDataCheckInterval;
        private int openConnectionTimeout = UdpRtpUrl.DefaultUdpOpenConnectionTimeout;
        private int openConnectionSleepTime = UdpRtpUrl.DefaultUdpOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = UdpRtpUrl.DefaultUdpTotalReopenConnectionTimeout;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UdpRtpUrl"/> class.
        /// </summary>
        /// <param name="url">The URL to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="UdpRtpUrl"/> class.
        /// </overloads>
        public UdpRtpUrl(String url)
            : this(new Uri(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UdpRtpUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier.</param>
        public UdpRtpUrl(Uri uri)
            : base(uri)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timeout to open UDP or RTP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The timeout to open url in milliseconds."), DefaultValue(UdpRtpUrl.DefaultUdpOpenConnectionTimeout)]
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
        [Category("Connection"), Description("The time in milliseconds to sleep before opening connection. UDP multicast protocol sometimes needs additional time to subscribe and unsubscribe from multicast group. In that case is recommended 200 milliseconds."), DefaultValue(UdpRtpUrl.DefaultUdpOpenConnectionSleepTime)]
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
        /// Gets or sets the total timeout to open UDP or RTP url in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="TotalReopenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The total timeout to open url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored."), DefaultValue(UdpRtpUrl.DefaultUdpTotalReopenConnectionTimeout)]
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
        
        /// <summary>
        /// Gets or sets the receive data check interval.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ReceiveDataCheckInterval"/> is lower than zero.</para>
        /// </exception>
        [Category("UDP or RTP"), Description("The time in milliseconds to check incomming data. If count of incoming data is same, then connection is assumed as lost and is closed and opened new connection."), DefaultValue(UdpRtpUrl.DefaultUdpReceiveDataCheckInterval)]
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

        /// <summary>
        /// Gets canonical string representation for the specified instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the unescaped canonical representation of the this instance.
        /// </returns>
        public override string ToString()
        {
            ParameterCollection parameters = new ParameterCollection();

            if (this.ReceiveDataCheckInterval != UdpRtpUrl.DefaultUdpReceiveDataCheckInterval)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpReceiveDataCheckInterval, this.ReceiveDataCheckInterval.ToString()));
            }
            if (this.OpenConnectionTimeout != UdpRtpUrl.DefaultUdpOpenConnectionTimeout)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpOpenConnectionTimeout, this.OpenConnectionTimeout.ToString()));
            }
            if (this.OpenConnectionSleepTime != UdpRtpUrl.DefaultUdpOpenConnectionSleepTime)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpOpenConnectionSleepTime, this.OpenConnectionSleepTime.ToString()));
            }
            if (this.TotalReopenConnectionTimeout != UdpRtpUrl.DefaultUdpTotalReopenConnectionTimeout)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpTotalReopenConnectionTimeout, this.TotalReopenConnectionTimeout.ToString()));
            }

            // return formatted connection string
            return base.ToString() + ParameterCollection.ParameterSeparator + parameters.FilterParameters;
        }

        /// <summary>
        /// Parses parameters from URL to current instance.
        /// </summary>
        /// <param name="parameters">The parameters from URL.</param>
        public override void Parse(ParameterCollection parameters)
        {
            base.Parse(parameters);

            foreach (var param in parameters)
            {
                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpOpenConnectionTimeout) == 0)
                {
                    this.OpenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpOpenConnectionSleepTime) == 0)
                {
                    this.OpenConnectionSleepTime = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpTotalReopenConnectionTimeout) == 0)
                {
                    this.TotalReopenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpReceiveDataCheckInterval) == 0)
                {
                    this.ReceiveDataCheckInterval = int.Parse(param.Value);
                }
            }
        }

        public override void ApplyDefaultUserSettings(ProtocolSettings previousSettings, ProtocolSettings currentSettings)
        {
            base.ApplyDefaultUserSettings(previousSettings, currentSettings);

            UdpRtpProtocolSettings udpRtpPreviousSettings = (UdpRtpProtocolSettings)previousSettings;
            UdpRtpProtocolSettings udpRtpCurrentSettings = (UdpRtpProtocolSettings)currentSettings;

            if ((this.ReceiveDataCheckInterval == UdpRtpUrl.DefaultUdpReceiveDataCheckInterval) ||
                (this.ReceiveDataCheckInterval == udpRtpPreviousSettings.ReceiveDataCheckInterval))
            {
                this.ReceiveDataCheckInterval = udpRtpCurrentSettings.ReceiveDataCheckInterval;
            }

            if ((this.OpenConnectionTimeout == UdpRtpUrl.DefaultUdpOpenConnectionTimeout) ||
                (this.OpenConnectionTimeout == udpRtpPreviousSettings.OpenConnectionTimeout))
            {
                this.OpenConnectionTimeout = udpRtpCurrentSettings.OpenConnectionTimeout;
            }

            if ((this.OpenConnectionSleepTime == UdpRtpUrl.DefaultUdpOpenConnectionSleepTime) ||
                (this.OpenConnectionSleepTime == udpRtpPreviousSettings.OpenConnectionSleepTime))
            {
                this.OpenConnectionSleepTime = udpRtpCurrentSettings.OpenConnectionSleepTime;
            }

            if ((this.TotalReopenConnectionTimeout == UdpRtpUrl.DefaultUdpTotalReopenConnectionTimeout) ||
                (this.TotalReopenConnectionTimeout == udpRtpPreviousSettings.TotalReopenConnectionTimeout))
            {
                this.TotalReopenConnectionTimeout = udpRtpCurrentSettings.TotalReopenConnectionTimeout;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies open connection timeout in milliseconds.
        /// </summary>
        protected static readonly String ParameterUdpOpenConnectionTimeout = "UdpOpenConnectionTimeout";

        /// <summary>
        /// Specifies the time in milliseconds to sleep before opening connection.
        /// </summary>
        protected static readonly String ParameterUdpOpenConnectionSleepTime = "UdpOpenConnectionSleepTime";

        /// <summary>
        /// Specifies the total timeout to open UDP or RTP url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.
        /// </summary>
        protected static readonly String ParameterUdpTotalReopenConnectionTimeout = "UdpTotalReopenConnectionTimeout";

        /// <summary>
        /// Specifies receive data check interval.
        /// </summary>
        protected static readonly String ParameterUdpReceiveDataCheckInterval = "UdpReceiveDataCheckInterval";

        /// <summary>
        /// Default value for <see cref="ParameterUdpOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultUdpOpenConnectionTimeout = 2000;

        /// <summary>
        /// Default value for <see cref="ParameterUdpOpenConnectionSleepTime"/>.
        /// </summary>
        public const int DefaultUdpOpenConnectionSleepTime = 0;

        /// <summary>
        /// Default value for <see cref="ParameterUdpTotalReopenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultUdpTotalReopenConnectionTimeout = 60000;

        /// <summary>
        /// Default receive data check interval.
        /// </summary>
        public const int DefaultUdpReceiveDataCheckInterval = 500;

        #endregion
    }
}
