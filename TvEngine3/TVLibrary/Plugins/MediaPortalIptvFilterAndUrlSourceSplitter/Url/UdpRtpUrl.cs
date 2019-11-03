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

        private int dscp = UdpRtpUrl.DefaultUdpDscp;
        private int ecn = UdpRtpUrl.DefaultUdpEcn;
        private int identification = UdpRtpUrl.DefaultIdentification;
        private Boolean dontFragment = UdpRtpUrl.DefaultUdpDontFragment;
        private Boolean moreFragments = UdpRtpUrl.DefaultUdpMoreFragments;
        private int ttl = UdpRtpUrl.DefaultUdpTtl;
        private String options = UdpRtpUrl.DefaultUdpOptions;

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

        /* very specific UDP (RTP) options */

        /// <summary>
        /// Gets or sets the DSCP value of IPv4 header.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="Dscp"/> is lower than zero or greater than 63.</para>
        /// </exception>
        [Category("UDP or RTP (raw)"), Description("DSCP"), DefaultValue(UdpRtpUrl.DefaultUdpDscp)]
        public int Dscp
        {
            get { return this.dscp; }
            set
            {
                if ((value < 0) || (value > 63))
                {
                    throw new ArgumentOutOfRangeException("Dscp", value, "Cannot be less than zero or greater than 63.");
                }

                this.dscp = value;
            }
        }

        /// <summary>
        /// Gets or sets the ECN value of IPv4 header.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="Ecn"/> is lower than zero or greater than 3.</para>
        /// </exception>
        [Category("UDP or RTP (raw)"), Description("ECN"), DefaultValue(UdpRtpUrl.DefaultUdpEcn)]
        public int Ecn
        {
            get { return this.ecn; }
            set
            {
                if ((value < 0) || (value > 3))
                {
                    throw new ArgumentOutOfRangeException("Ecn", value, "Cannot be less than zero or greater than 3.");
                }

                this.ecn = value;
            }
        }

        /// <summary>
        /// Gets or sets the identification value of IPv4 header.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="Identification"/> is lower than -1 or greater than 65535.</para>
        /// </exception>
        [Category("UDP or RTP (raw)"), Description("Identification"), DefaultValue(UdpRtpUrl.DefaultIdentification)]
        public int Identification
        {
            get { return this.identification; }
            set
            {
                if ((value < -1) || (value > 65535))
                {
                    throw new ArgumentOutOfRangeException("Identification", value, "Cannot be less than -1 or greater than 65535.");
                }

                this.identification = value;
            }
        }

        /// <summary>
        /// Gets or sets the dont fragment flag of IPv4 header.
        /// </summary>
        [Category("UDP or RTP (raw)"), Description("DontFragment"), DefaultValue(UdpRtpUrl.DefaultUdpDontFragment)]
        public Boolean DontFragment
        {
            get { return this.dontFragment; }
            set { this.dontFragment = value; }
        }

        /// <summary>
        /// Gets or sets the more fragments flag of IPv4 header.
        /// </summary>
        [Category("UDP or RTP (raw)"), Description("MoreFragments"), DefaultValue(UdpRtpUrl.DefaultUdpMoreFragments)]
        public Boolean MoreFragments
        {
            get { return this.moreFragments; }
            set { this.moreFragments = value; }
        }

        /// <summary>
        /// Gets or sets the TTL value of IPv4 header.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ttl"/> is lower than zero or greater than 255.</para>
        /// </exception>
        [Category("UDP or RTP (raw)"), Description("TTL"), DefaultValue(UdpRtpUrl.DefaultUdpTtl)]
        public int Ttl
        {
            get { return this.ttl; }
            set
            {
                if ((value < 0) || (value > 255))
                {
                    throw new ArgumentOutOfRangeException("Ttl", value, "Cannot be less than 0 or greater than 255.");
                }

                this.ttl = value;
            }
        }

        /// <summary>
        /// Gets or sets the options field of IPv4 header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Options"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The length of <see cref="Options"/> is not dividable by 2 without remainder.</para>
        /// </exception>
        /// <exception cref="FormatException">
        /// <para>The <see cref="Options"/> is not valid HEX string.</para>
        /// </exception>
        [Category("UDP or RTP (raw)"), Description("Options"), DefaultValue(UdpRtpUrl.DefaultUdpOptions)]
        public String Options
        {
            get { return this.options; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Options");
                }

                if ((value.Length % 2) != 0)
                {
                    throw new ArgumentOutOfRangeException("Options", value, "The length must be dividable by 2 without remainder.");
                }

                UdpRtpUrl.HexadecimalStringToByteArray(value);

                this.options = value;
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

            if (this.Dscp != UdpRtpUrl.DefaultUdpDscp)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpDscp, this.Dscp.ToString()));
            }
            if (this.Ecn != UdpRtpUrl.DefaultUdpEcn)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpEcn, this.Ecn.ToString()));
            }
            if (this.Identification != UdpRtpUrl.DefaultIdentification)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpIdentification, this.Identification.ToString()));
            }
            if (this.DontFragment != UdpRtpUrl.DefaultUdpDontFragment)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpDontFragment, this.DontFragment ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.MoreFragments != UdpRtpUrl.DefaultUdpMoreFragments)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpMoreFragments, this.MoreFragments ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.Ttl != UdpRtpUrl.DefaultUdpTtl)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpTtl, this.Ttl.ToString()));
            }
            if (String.CompareOrdinal(this.Options, UdpRtpUrl.DefaultUdpOptions) != 0)
            {
                parameters.Add(new Parameter(UdpRtpUrl.ParameterUdpOptions, this.Options));
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

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpDscp) == 0)
                {
                    this.Dscp = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpEcn) == 0)
                {
                    this.Ecn = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpIdentification) == 0)
                {
                    this.Identification = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpDontFragment) == 0)
                {
                    this.DontFragment = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpMoreFragments) == 0)
                {
                    this.MoreFragments = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpTtl) == 0)
                {
                    this.Ttl = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, UdpRtpUrl.ParameterUdpOptions) == 0)
                {
                    this.Options = param.Value;
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

        private static Byte[] HexadecimalStringToByteArray(String input)
        {
            var outputLength = input.Length / 2;
            var output = new byte[outputLength];
            using (var sr = new System.IO.StringReader(input))
            {
                for (var i = 0; i < outputLength; i++)
                {
                    output[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
                }
            }
            return output;
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

        /* very specific UDP options, all of them requires that user is member or Administrators group (due to using raw sockets) */

        /// <summary>
        /// Specifies DSCP value of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpDscp = "UdpDscp";

        /// <summary>
        /// Specifies ECN value of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpEcn = "UdpEcn";

        /// <summary>
        /// Specifies identification value of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpIdentification = "UdpIdentification";

        /// <summary>
        /// Specifies don't fragment flag of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpDontFragment = "UdpDontFragment";

        /// <summary>
        /// Specifies more fragments flag of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpMoreFragments = "UdpMoreFragments";

        /// <summary>
        /// Specifies TTL value of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpTtl = "UdpTtl";

        /// <summary>
        /// Specifies options field of IPv4 header.
        /// </summary>
        protected static readonly String ParameterUdpOptions = "UdpOptions";

        /* default values */

        /// <summary>
        /// Default value for <see cref="ParameterUdpOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultUdpOpenConnectionTimeout = 1000;

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

        /* specific udp options */

        /// <summary>
        /// Default value for <see cref="ParameterUdpDscp"/>.
        /// </summary>
        public const int DefaultUdpDscp = 0;

        /// <summary>
        /// Default value for <see cref="ParameterUdpEcn"/>.
        /// </summary>
        public const int DefaultUdpEcn = 0;

        /// <summary>
        /// Default value for <see cref="ParameterUdpIdentification"/>.
        /// </summary>
        public const int DefaultIdentification = -1;

        /// <summary>
        /// Default value for <see cref="ParameterUdpDontFragment"/>.
        /// </summary>
        public const Boolean DefaultUdpDontFragment = false;

        /// <summary>
        /// Default value for <see cref="ParameterUdpMoreFragments"/>.
        /// </summary>
        public const Boolean DefaultUdpMoreFragments = false;

        /// <summary>
        /// Default value for <see cref="ParameterUdpTtl"/>.
        /// </summary>
        public const int DefaultUdpTtl = 32;

        /// <summary>
        /// Default value for <see cref="ParameterUdpOptions"/>.
        /// </summary>
        public const String DefaultUdpOptions = "";

        #endregion
    }
}
