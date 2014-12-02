using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represent base class for RTSP urls for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class RtspUrl : SimpleUrl
    {
        #region Private fields

        private int multicastPreference = RtspUrl.DefaultRtspMulticastPreference;
        private int udpPreference = RtspUrl.DefaultRtspUdpPreference;
        private int sameConnectionPreference = RtspUrl.DefaultRtspSameConnectionTcpPreference;

        private int openConnectionTimeout = RtspUrl.DefaultRtspOpenConnectionTimeout;
        private int openConnectionSleepTime = RtspUrl.DefaultRtspOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = RtspUrl.DefaultRtspTotalReopenConnectionTimeout;

        private int clientPortMin = RtspUrl.DefaultRtspClientPortMin;
        private int clientPortMax = RtspUrl.DefaultRtspClientPortMax;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtspUrl"/> class.
        /// </summary>
        /// <param name="url">The URL to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtspUrl"/> class.
        /// </overloads>
        public RtspUrl(String url)
            : this(new Uri(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtspUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier.</param>
        public RtspUrl(Uri uri)
            : base(uri)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timeout to open RTSP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The timeout to open url in milliseconds."), DefaultValue(RtspUrl.DefaultRtspOpenConnectionTimeout)]
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
        [Category("Connection"), Description("The time in milliseconds to sleep before opening connection."), DefaultValue(RtspUrl.DefaultRtspOpenConnectionSleepTime)]
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
        /// Gets or sets the total timeout to open RTSP url in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="TotalReopenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The total timeout to open url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored."), DefaultValue(RtspUrl.DefaultRtspTotalReopenConnectionTimeout)]
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
        /// Gets or sets the minimum client port for UDP connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMin"/> is lower than zero.</para>
        /// <para>The <see cref="ClientPortMin"/> is greater than 65535.</para>
        /// <para>The <see cref="ClientPortMin"/> is greater than <see cref="ClientPortMax"/>.</para>
        /// </exception>
        [Category("RTSP"), Description("The minimum UDP port to be used in UDP transport."), DefaultValue(RtspUrl.DefaultRtspClientPortMin)]
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
                if (value >= this.ClientPortMax)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be greater than maximum client port.");
                }

                this.clientPortMin = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum client port for UDP connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMax"/> is lower than zero.</para>
        /// <para>The <see cref="ClientPortMax"/> is greater than 65535.</para>
        /// <para>The <see cref="ClientPortMax"/> is lower than <see cref="ClientPortMin"/>.</para>
        /// </exception>
        [Category("RTSP"), Description("The maximum UDP port to be used in UDP transport."), DefaultValue(RtspUrl.DefaultRtspClientPortMax)]
        public int ClientPortMax
        {
            get { return this.clientPortMax; }
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
                if (value <= this.ClientPortMin)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be lower than minimum client port.");
                }

                this.clientPortMax = value;
            }
        }

        /// <summary>
        /// Specifies UDP multicast connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="MulticastPreference"/> is lower than zero.</para>
        /// </exception>
        [Editor(typeof(RtspConnectionPreferenceEditor), typeof(UITypeEditor))]
        [Category("RTSP connection"), Description("The preference of multicast UDP transport."), DefaultValue(RtspUrl.DefaultRtspMulticastPreference)]
        public int MulticastPreference
        {
            get { return this.multicastPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MulticastPreference", value, "Cannot be lower than zero.");
                }

                this.multicastPreference = value;
            }
        }

        /// <summary>
        /// Specifies UDP connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="UdpPreference"/> is lower than zero.</para>
        /// </exception>
        [Editor(typeof(RtspConnectionPreferenceEditor), typeof(UITypeEditor))]
        [Category("RTSP connection"), Description("The preference of unicast UDP transport."), DefaultValue(RtspUrl.DefaultRtspUdpPreference)]
        public int UdpPreference
        {
            get { return this.udpPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("UdpPreference", value, "Cannot be lower than zero.");
                }

                this.udpPreference = value;
            }
        }

        /// <summary>
        /// Specifies same connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="SameConnectionPreference"/> is lower than zero.</para>
        /// </exception>
        [Editor(typeof(RtspConnectionPreferenceEditor), typeof(UITypeEditor))]
        [Category("RTSP connection"), Description("The preference of interleaved TCP transport."), DefaultValue(RtspUrl.DefaultRtspSameConnectionTcpPreference)]
        public int SameConnectionPreference
        {
            get { return this.sameConnectionPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SameConnectionPreference", value, "Cannot be lower than zero.");
                }

                this.sameConnectionPreference = value;
            }
        }

        /// <summary>
        /// Specifies ignore payload type flag.
        /// </summary>
        [Category("RTSP"), Description("Specifies if filter have to ignore check of received RTP packets payload type against payload type specified in SDP."), DefaultValue(RtspUrl.DefaultRtspIgnoreRtpPayloadType)]
        public Boolean IgnorePayloadType { get; set; }

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

            if (this.ClientPortMax != RtspUrl.DefaultRtspClientPortMax)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspClientPortMax, this.ClientPortMax.ToString()));
            }
            if (this.ClientPortMin != RtspUrl.DefaultRtspClientPortMin)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspClientPortMin, this.ClientPortMin.ToString()));
            }
            if (this.OpenConnectionTimeout != RtspUrl.DefaultRtspOpenConnectionTimeout)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspOpenConnectionTimeout, this.OpenConnectionTimeout.ToString()));
            }
            if (this.OpenConnectionSleepTime != RtspUrl.DefaultRtspOpenConnectionSleepTime)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspOpenConnectionSleepTime, this.OpenConnectionSleepTime.ToString()));
            }
            if (this.TotalReopenConnectionTimeout != RtspUrl.DefaultRtspTotalReopenConnectionTimeout)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspTotalReopenConnectionTimeout, this.TotalReopenConnectionTimeout.ToString()));
            }
            if (this.IgnorePayloadType != RtspUrl.DefaultRtspIgnoreRtpPayloadType)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspIgnoreRtpPayloadType, this.IgnorePayloadType ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.MulticastPreference != RtspUrl.DefaultRtspMulticastPreference)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspMulticastPreference, this.MulticastPreference.ToString()));
            }
            if (this.SameConnectionPreference != RtspUrl.DefaultRtspSameConnectionTcpPreference)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspSameConnectionTcpPreference, this.SameConnectionPreference.ToString()));
            }
            if (this.UdpPreference != RtspUrl.DefaultRtspUdpPreference)
            {
                parameters.Add(new Parameter(RtspUrl.ParameterRtspUdpPreference, this.UdpPreference.ToString()));
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
                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspOpenConnectionTimeout) == 0)
                {
                    this.OpenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspOpenConnectionSleepTime) == 0)
                {
                    this.OpenConnectionSleepTime = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspTotalReopenConnectionTimeout) == 0)
                {
                    this.TotalReopenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspMulticastPreference) == 0)
                {
                    this.MulticastPreference = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspUdpPreference) == 0)
                {
                    this.UdpPreference = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspSameConnectionTcpPreference) == 0)
                {
                    this.SameConnectionPreference = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspIgnoreRtpPayloadType) == 0)
                {
                    this.IgnorePayloadType = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspClientPortMin) == 0)
                {
                    this.ClientPortMin = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtspUrl.ParameterRtspClientPortMax) == 0)
                {
                    this.ClientPortMax = int.Parse(param.Value);
                }
            }
        }

        public override void ApplyDefaultUserSettings(ProtocolSettings previousSettings, ProtocolSettings currentSettings)
        {
            base.ApplyDefaultUserSettings(previousSettings, currentSettings);

            RtspProtocolSettings rtspPreviousSettings = (RtspProtocolSettings)previousSettings;
            RtspProtocolSettings rtspCurrentSettings = (RtspProtocolSettings)currentSettings;

            if ((this.OpenConnectionTimeout == RtspUrl.DefaultRtspOpenConnectionTimeout) ||
                (this.OpenConnectionTimeout == rtspPreviousSettings.OpenConnectionTimeout))
            {
                this.OpenConnectionTimeout = rtspCurrentSettings.OpenConnectionTimeout;
            }

            if ((this.OpenConnectionSleepTime == RtspUrl.DefaultRtspOpenConnectionSleepTime) ||
                (this.OpenConnectionSleepTime == rtspPreviousSettings.OpenConnectionSleepTime))
            {
                this.OpenConnectionSleepTime = rtspCurrentSettings.OpenConnectionSleepTime;
            }

            if ((this.TotalReopenConnectionTimeout == RtspUrl.DefaultRtspTotalReopenConnectionTimeout) ||
                (this.TotalReopenConnectionTimeout == rtspPreviousSettings.TotalReopenConnectionTimeout))
            {
                this.TotalReopenConnectionTimeout = rtspCurrentSettings.TotalReopenConnectionTimeout;
            }

            if ((this.MulticastPreference == RtspUrl.DefaultRtspMulticastPreference) ||
                (this.MulticastPreference == rtspPreviousSettings.MulticastPreference))
            {
                this.MulticastPreference = rtspCurrentSettings.MulticastPreference;
            }

            if ((this.UdpPreference == RtspUrl.DefaultRtspUdpPreference) ||
                (this.UdpPreference == rtspPreviousSettings.UdpPreference))
            {
                this.UdpPreference = rtspCurrentSettings.UdpPreference;
            }

            if ((this.SameConnectionPreference == RtspUrl.DefaultRtspSameConnectionTcpPreference) ||
                (this.SameConnectionPreference == rtspPreviousSettings.SameConnectionPreference))
            {
                this.SameConnectionPreference = rtspCurrentSettings.SameConnectionPreference;
            }

            if ((this.IgnorePayloadType == RtspUrl.DefaultRtspIgnoreRtpPayloadType) ||
                (this.IgnorePayloadType == rtspPreviousSettings.IgnoreRtpPayloadType))
            {
                this.IgnorePayloadType = rtspCurrentSettings.IgnoreRtpPayloadType;
            }

            if ((this.ClientPortMin == RtspUrl.DefaultRtspClientPortMin) ||
                (this.ClientPortMin == rtspPreviousSettings.ClientPortMin))
            {
                this.ClientPortMin = rtspCurrentSettings.ClientPortMin;
            }

            if ((this.ClientPortMax == RtspUrl.DefaultRtspClientPortMax) ||
                (this.ClientPortMax == rtspPreviousSettings.ClientPortMax))
            {
                this.ClientPortMax = rtspCurrentSettings.ClientPortMax;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies open connection timeout in milliseconds.
        /// </summary>
        protected static readonly String ParameterRtspOpenConnectionTimeout = "RtspOpenConnectionTimeout";

        /// <summary>
        /// Specifies the time in milliseconds to sleep before opening connection.
        /// </summary>
        protected static readonly String ParameterRtspOpenConnectionSleepTime = "RtspOpenConnectionSleepTime";

        /// <summary>
        /// Specifies the total timeout to open RTSP url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.
        /// </summary>
        protected static readonly String ParameterRtspTotalReopenConnectionTimeout = "RtspTotalReopenConnectionTimeout";

        /// <summary>
        /// Specifies UDP multicast connection preference of RTSP protocol.
        /// </summary>
        protected static readonly String ParameterRtspMulticastPreference = "RtspMulticastPreference";

        /// <summary>
        /// Specifies UDP connection preference of RTSP protocol.
        /// </summary>
        protected static readonly String ParameterRtspUdpPreference = "RtspUdpPreference";

        /// <summary>
        /// Specifies same connection preference of RTSP protocol.
        /// </summary>
        protected static readonly String ParameterRtspSameConnectionTcpPreference = "RtspSameConnectionTcpPreference";

        /// <summary>
        /// Specifies ignore RTP payload type flag of RTSP protocol.
        /// </summary>
        protected static readonly String ParameterRtspIgnoreRtpPayloadType = "RtspIgnoreRtpPayloadType";

        /// <summary>
        /// Specifies minimum client port for UDP connection.
        /// </summary>
        protected static readonly String ParameterRtspClientPortMin = "RtspClientPortMin";

        /// <summary>
        /// Specifies maximum client port for UDP connection.
        /// </summary>
        protected static readonly String ParameterRtspClientPortMax = "RtspClientPortMax";

        /// <summary>
        /// Default value for <see cref="ParameterRtspOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultRtspOpenConnectionTimeout = 1500;

        /// <summary>
        /// Default value for <see cref="ParameterRtspOpenConnectionSleepTime"/>.
        /// </summary>
        public const int DefaultRtspOpenConnectionSleepTime = 0;

        /// <summary>
        /// Default value for <see cref="ParameterRtspTotalReopenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultRtspTotalReopenConnectionTimeout = 60000;

        /// <summary>
        /// Default UDP multicast connection preference.
        /// </summary>
        public const int DefaultRtspMulticastPreference = 2;

        /// <summary>
        /// Default UDP connection preference.
        /// </summary>
        public const int DefaultRtspUdpPreference = 1;

        /// <summary>
        /// Default same connection preference.
        /// </summary>
        public const int DefaultRtspSameConnectionTcpPreference = 0;

        /// <summary>
        /// Default ignore payload type flag.
        /// </summary>
        public const Boolean DefaultRtspIgnoreRtpPayloadType = false;

        /// <summary>
        /// Default minimum client port for UDP connection.
        /// </summary>
        public const int DefaultRtspClientPortMin = 50000;

        /// <summary>
        /// Default maximum client port for UDP connection.
        /// </summary>
        public const int DefaultRtspClientPortMax = 65535;

        #endregion
    }
}
