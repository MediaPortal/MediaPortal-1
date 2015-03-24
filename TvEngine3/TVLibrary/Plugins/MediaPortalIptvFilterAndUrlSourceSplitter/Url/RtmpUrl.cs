using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represent class for RTMP urls for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class RtmpUrl : SimpleUrl
    {
        #region Private fields

        private int openConnectionTimeout = RtmpUrl.DefaultRtmpOpenConnectionTimeout;
        private int openConnectionSleepTime = RtmpUrl.DefaultRtmpOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = RtmpUrl.DefaultRtmpTotalReopenConnectionTimeout;
        private RtmpArbitraryDataCollection arbitraryData;
        private int bufferTime = RtmpUrl.DefaultBufferTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpUrl"/> class.
        /// </summary>
        /// <param name="url">The URL to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="RtmpUrl"/> class.
        /// </overloads>
        public RtmpUrl(String url)
            : this(new Uri(url))
        {
        }

        public RtmpUrl(string tcUrl, string hostname, int port)
            : this(new Uri((!string.IsNullOrEmpty(tcUrl) ? new Uri(tcUrl).Scheme : "rtmp") + "://" + hostname + (port > 0 ? ":" + port : "")))
        {
            this.TcUrl = tcUrl;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier.</param>
        public RtmpUrl(Uri uri)
            : base(uri)
        {
            this.App = RtmpUrl.DefaultApp;
            this.TcUrl = RtmpUrl.DefaultTcUrl;
            this.PageUrl = RtmpUrl.DefaultPageUrl;
            this.SwfUrl = RtmpUrl.DefaultSwfUrl;
            this.FlashVersion = RtmpUrl.DefaultFlashVersion;
            this.PlayPath = RtmpUrl.DefaultPlayPath;
            this.Playlist = RtmpUrl.DefaultPlaylist;
            this.Live = RtmpUrl.DefaultLive;
            this.Subscribe = RtmpUrl.DefaultSubscribe;
            this.BufferTime = RtmpUrl.DefaultBufferTime;
            this.Token = RtmpUrl.DefaultToken;
            this.Jtv = RtmpUrl.DefaultJtv;
            this.SwfVerify = RtmpUrl.DefaultSwfVerify;
            this.arbitraryData = new RtmpArbitraryDataCollection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of application to connect to on the RTMP server.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If not <see langword="null"/> then overrides the app in the RTMP URL.
        /// Sometimes the librtmp URL parser cannot determine the app name automatically,
        /// so it must be given explicitly using this option.
        /// </para>
        /// <para>
        /// The default value is <see langword="null"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("Name of application to connect to on the RTMP server."), DefaultValue(RtmpUrl.DefaultApp)]
        public String App { get; set; }

        /// <summary>
        /// Gets arbitray data collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default value is empty collection.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("Specifies arbitrary AMF data to Connect message.")]
        [Editor(typeof(RtmpArbitraryDataCollectionEditor), typeof(UITypeEditor))]
        public RtmpArbitraryDataCollection ArbitraryData
        {
            get { return this.arbitraryData; }
        }

        /// <summary>
        /// Gets or sets the URL of the target stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If is <see langword="null"/> then rtmp[t][e|s]://host[:port]/app is used.
        /// </para>
        /// <para>
        /// The default value is <see langword="null"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("URL of the target stream."), DefaultValue(RtmpUrl.DefaultTcUrl)]
        public String TcUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the web page in which the media was embedded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If is <see langword="null"/> then no value will be sent.
        /// </para>
        /// <para>
        /// The default value is <see langword="null"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("URL of the web page in which the media was embedded."), DefaultValue(RtmpUrl.DefaultPageUrl)]
        public String PageUrl { get; set; }

        /// <summary>
        /// Gets or sets URL of the SWF player for the media.
        /// </summary>
        /// <remarks>
        /// <para>If is <see langword="null"/> then no value will be sent.</para>
        /// <para>The default value is <see langword="null"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("URL of the SWF player for the media."), DefaultValue(RtmpUrl.DefaultSwfUrl)]
        public String SwfUrl { get; set; }

        /// <summary>
        /// Gets or sets the version of the Flash plugin used to run the SWF player.
        /// </summary>
        /// <remarks>
        /// <para>If is <see langword="null"/> then "WIN 10,0,32,18" is sent.</para>
        /// <para>The default value is <see langword="null"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("Version of the Flash plugin used to run the SWF player."), DefaultValue(RtmpUrl.DefaultFlashVersion)]
        public String FlashVersion { get; set; }

        /// <summary>
        /// Gets or sets the authentication string to be appended to the connect string.
        /// </summary>
        /// <remarks>
        /// <para>The default value is <see langword="null"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("Authentication string to be appended to the connect string."), DefaultValue(RtmpUrl.DefaultAuth)]
        public String Auth { get; set; }

        /// <summary>
        /// Gets or sets the playpath.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If not <see langword="null"/> then overrides the playpath parsed from the RTMP URL.
        /// Sometimes the librtmp URL parser cannot determine the correct playpath automatically,
        /// so it must be given explicitly using this option.
        /// </para>
        /// <para>The default value is <see langword="null"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("The play path of media."), DefaultValue(RtmpUrl.DefaultPlayPath)]
        public String PlayPath { get; set; }

        /// <summary>
        /// Gets or sets if set_playlist command have to be sent before play command.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is <see langword="true"/>, issue a set_playlist command before sending the play command.
        /// The playlist will just contain the current playpath.
        /// If the value is <see langword="false"/>, the set_playlist command will not be sent.
        /// </para>
        /// <para>The default value is <see langword="false"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("Specifies to issue a set_playlist command before sending the play command."), DefaultValue(RtmpUrl.DefaultPlaylist)]
        public Boolean Playlist { get; set; }

        /// <summary>
        /// Specify that the media is a live stream.
        /// </summary>
        /// <remarks>
        /// <para>No resuming or seeking in live streams is possible.</para>
        /// <para>The default value is <see langword="false"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("Specifies that the media is a live stream."), DefaultValue(RtmpUrl.DefaultLive)]
        public Boolean Live { get; set; }

        /// <summary>
        /// Gets or sets the name of live stream to subscribe to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defaults to playpath.
        /// </para>
        /// <para>
        /// The default value is <see langword="null"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("Name of live stream to subscribe to."), DefaultValue(RtmpUrl.DefaultSubscribe)]
        public String Subscribe { get; set; }

        /// <summary>
        /// Gets or sets the buffer time.
        /// </summary>
        /// <remarks>
        /// <para>Buffer time is in milliseconds.</para>
        /// <para>The default value is <see cref="RtmpUrl.DefaultBufferTime"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("Sets buffer time to specified value in milliseconds."), DefaultValue(RtmpUrl.DefaultBufferTime)]
        public int BufferTime
        {
            get { return this.bufferTime; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("BufferTime", value, "Must be greater than or equal to zero.");
                }

                this.bufferTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the key for SecureToken response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used if the server requires SecureToken authentication.
        /// </para>
        /// <para>
        /// The default value is <see langword="null"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("Key for SecureToken response, used if the server requires SecureToken authentication."), DefaultValue(RtmpUrl.DefaultToken)]
        public String Token { get; set; }

        /// <summary>
        /// Gets or sets the JSON token used by legacy Justin.tv servers.
        /// </summary>
        /// <remarks>
        /// <para>JSON token used by legacy Justin.tv servers. Invokes NetStream.Authenticate.UsherToken.</para>
        /// <para>The default value is <see langword="null"/>.</para>
        /// </remarks>
        [Category("RTMP"), Description("JSON token used by legacy Justin.tv servers. Invokes NetStream.Authenticate.UsherToken."), DefaultValue(RtmpUrl.DefaultJtv)]
        public String Jtv { get; set; }

        /// <summary>
        /// Gets or sets if the SWF player have to be retrieved from <see cref="RtmpUrl.SwfUrl"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is <see langword="true"/>, the SWF player is retrieved from the specified <see cref="RtmpUrl.SwfUrl"/>
        /// for performing SWF verification. The SWF hash and size (used in the verification step) are computed automatically.
        /// Also the SWF information is cached in a .swfinfo file in the user's home directory,
        /// so that it doesn't need to be retrieved and recalculated every time.
        /// The .swfinfo file records the SWF URL, the time it was fetched,
        /// the modification timestamp of the SWF file, its size, and its hash.
        /// By default, the cached info will be used for 30 days before re-checking. 
        /// </para>
        /// <para>
        /// The default value is <see cref="RtmpUrl.DefaultSwfVerify"/>.
        /// </para>
        /// </remarks>
        [Category("RTMP"), Description("Specifies if the SWF player is retrieved from the specified RtmpSwfUrl for performing SWF Verification. The SWF hash and size (used in the verification step) are computed automatically."), DefaultValue(RtmpUrl.DefaultSwfVerify)]
        public Boolean SwfVerify { get; set; }

        /// <summary>
        /// Gets or sets the timeout to open RTMP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The timeout to open url in milliseconds."), DefaultValue(RtmpUrl.DefaultRtmpOpenConnectionTimeout)]
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
        [Category("Connection"), Description("The time in milliseconds to sleep before opening connection."), DefaultValue(RtmpUrl.DefaultRtmpOpenConnectionSleepTime)]
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
        [Category("Connection"), Description("The total timeout to open url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored."), DefaultValue(RtmpUrl.DefaultRtmpTotalReopenConnectionTimeout)]
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

        /// <summary>
        /// Gets canonical string representation for the specified instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the unescaped canonical representation of the this instance.
        /// </returns>
        public override string ToString()
        {
            ParameterCollection parameters = new ParameterCollection();

            if (this.App != RtmpUrl.DefaultApp)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterApp, this.App));
            }

            if (this.BufferTime != RtmpUrl.DefaultBufferTime)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterBufferTime, this.BufferTime.ToString()));
            }

            if (this.FlashVersion != RtmpUrl.DefaultFlashVersion)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterFlashVer, this.FlashVersion));
            }

            if (this.Auth != RtmpUrl.DefaultAuth)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterAuth, this.Auth));
            }

            if (this.ArbitraryData.Count != 0)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterArbitraryData, this.ArbitraryData.ToString()));
            }

            if (this.Jtv != RtmpUrl.DefaultJtv)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterJtv, this.Jtv));
            }

            if (this.Live != RtmpUrl.DefaultLive)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterLive, this.Live ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            if (this.OpenConnectionTimeout != RtmpUrl.DefaultRtmpOpenConnectionTimeout)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterRtmpOpenConnectionTimeout, this.OpenConnectionTimeout.ToString()));
            }

            if (this.OpenConnectionSleepTime != RtmpUrl.DefaultRtmpOpenConnectionSleepTime)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterRtmpOpenConnectionSleepTime, this.OpenConnectionSleepTime.ToString()));
            }

            if (this.TotalReopenConnectionTimeout != RtmpUrl.DefaultRtmpTotalReopenConnectionTimeout)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterRtmpTotalReopenConnectionTimeout, this.TotalReopenConnectionTimeout.ToString()));
            }

            if (this.PageUrl != RtmpUrl.DefaultPageUrl)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterPageUrl, this.PageUrl));
            }

            if (this.Playlist != RtmpUrl.DefaultPlaylist)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterPlaylist, this.Playlist ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            if (this.PlayPath != RtmpUrl.DefaultPlayPath)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterPlayPath, this.PlayPath));
            }

            if (this.Subscribe != RtmpUrl.DefaultSubscribe)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterSubscribe, this.Subscribe));
            }

            if (this.SwfVerify != RtmpUrl.DefaultSwfVerify)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterSwfVerify, this.SwfVerify ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            if (this.TcUrl != RtmpUrl.DefaultTcUrl)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterTcUrl, this.TcUrl));
            }

            if (this.Token != RtmpUrl.DefaultToken)
            {
                parameters.Add(new Parameter(RtmpUrl.ParameterToken, this.Token));
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
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterRtmpOpenConnectionTimeout) == 0)
                {
                    this.OpenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterRtmpOpenConnectionSleepTime) == 0)
                {
                    this.OpenConnectionSleepTime = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterRtmpTotalReopenConnectionTimeout) == 0)
                {
                    this.TotalReopenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterApp) == 0)
                {
                    this.App = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterTcUrl) == 0)
                {
                    this.TcUrl = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterPageUrl) == 0)
                {
                    this.PageUrl = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterSwfUrl) == 0)
                {
                    this.SwfUrl = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterFlashVer) == 0)
                {
                    this.FlashVersion = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterAuth) == 0)
                {
                    this.Auth = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterArbitraryData) == 0)
                {
                    this.ArbitraryData.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterPlayPath) == 0)
                {
                    this.PlayPath = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterPlaylist) == 0)
                {
                    this.Playlist = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterLive) == 0)
                {
                    this.Live = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterSubscribe) == 0)
                {
                    this.Subscribe = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterBufferTime) == 0)
                {
                    this.BufferTime = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterToken) == 0)
                {
                    this.Token = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterJtv) == 0)
                {
                    this.Jtv = param.Value;
                }
                if (String.CompareOrdinal(param.Name, RtmpUrl.ParameterSwfVerify) == 0)
                {
                    this.SwfVerify = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies open connection timeout in milliseconds.
        /// </summary>
        protected static readonly String ParameterRtmpOpenConnectionTimeout = "RtmpOpenConnectionTimeout";

        /// <summary>
        /// Specifies the time in milliseconds to sleep before opening connection.
        /// </summary>
        protected static readonly String ParameterRtmpOpenConnectionSleepTime = "RtmpOpenConnectionSleepTime";

        /// <summary>
        /// Specifies the total timeout to open RTMP url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.
        /// </summary>
        protected static readonly String ParameterRtmpTotalReopenConnectionTimeout = "RtmpTotalReopenConnectionTimeout";

        // connection parameters of RTMP protocol

        protected static String ParameterApp = "RtmpApp";

        protected static String ParameterTcUrl = "RtmpTcUrl";

        protected static String ParameterPageUrl = "RtmpPageUrl";

        protected static String ParameterSwfUrl = "RtmpSwfUrl";

        protected static String ParameterFlashVer = "RtmpFlashVer";

        protected static String ParameterAuth = "RtmpAuth";

        protected static String ParameterArbitraryData = "RtmpArbitraryData";

        // session parameters of RTMP protocol

        protected static String ParameterPlayPath = "RtmpPlayPath";

        protected static String ParameterPlaylist = "RtmpPlaylist";

        protected static String ParameterLive = "RtmpLive";

        protected static String ParameterSubscribe = "RtmpSubscribe";

        protected static String ParameterBufferTime = "RtmpBuffer";

        // security parameters of RTMP protocol

        protected static String ParameterToken = "RtmpToken";

        protected static String ParameterJtv = "RtmpJtv";

        protected static String ParameterSwfVerify = "RtmpSwfVerify";

        // default values for some parameters

        /// <summary>
        /// Default value for <see cref="ParameterRtmpOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultRtmpOpenConnectionTimeout = 20000;

        /// <summary>
        /// Default value for <see cref="ParameterRtmpOpenConnectionSleepTime"/>.
        /// </summary>
        public const int DefaultRtmpOpenConnectionSleepTime = 0;

        /// <summary>
        /// Default value for <see cref="ParameterHttpTotalReopenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultRtmpTotalReopenConnectionTimeout = 60000;

        public const String DefaultApp = null;
        public const String DefaultTcUrl = null;
        public const String DefaultPageUrl = null;
        public const String DefaultSwfUrl = null;
        public const String DefaultFlashVersion = null;
        public const String DefaultAuth = null;
        public const String DefaultPlayPath = null;
        public const Boolean DefaultPlaylist = false;
        public const Boolean DefaultLive = false;
        public const String DefaultSubscribe = null;
        public const int DefaultBufferTime = 30000;
        public const String DefaultToken = null;
        public const String DefaultJtv = null;
        public const Boolean DefaultSwfVerify = false;

        #endregion
    }
}
