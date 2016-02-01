using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represent base class for HTTP urls for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class HttpUrl : SimpleUrl
    {
        #region Private fields

        private String referer = HttpUrl.DefaultHttpReferer;
        private String userAgent = HttpUrl.DefaultHttpUserAgent;
        Version version = HttpUrl.DefaultHttpVersion;
        private int openConnectionTimeout = HttpUrl.DefaultHttpOpenConnectionTimeout;
        private int openConnectionSleepTime = HttpUrl.DefaultHttpOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = HttpUrl.DefaultHttpTotalReopenConnectionTimeout;
        private String cookie = HttpUrl.DefaultHttpCookie;

        private String serverUserName = HttpUrl.DefaultHttpServerUserName;
        private String serverPassword = HttpUrl.DefaultHttpServerPassword;

        private String proxyServer = HttpUrl.DefaultHttpProxyServer;
        private int proxyServerPort = HttpUrl.DefaultHttpProxyServerPort;
        private String proxyServerUserName = HttpUrl.DefaultHttpProxyServerUserName;
        private String proxyServerPassword = HttpUrl.DefaultHttpProxyServerPassword;
        private ProxyServerType proxyServerType = HttpUrl.DefaultHttpProxyServerType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </summary>
        /// <param name="url">The URL to initialize.</param>
        /// <overloads>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </overloads>
        public HttpUrl(String url)
            : this(new Uri(url))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HttpUrl"/> class.
        /// </summary>
        /// <param name="uri">The uniform resource identifier.</param>
        public HttpUrl(Uri uri)
            : base(uri)
        {
            this.Uri = new Uri(uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.UserInfo, UriFormat.UriEscaped));
            this.SeekingSupported = HttpUrl.DefaultHttpSeekingSupported;
            this.SeekingSupportDetection = HttpUrl.DefaultHttpSeekingSupportDetection;

            if (!String.IsNullOrWhiteSpace(uri.UserInfo))
            {
                String[] userNamePassword = uri.UserInfo.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                if (userNamePassword.Length == 2)
                {
                    this.ServerAuthenticate = true;
                    this.ServerUserName = userNamePassword[0];
                    this.ServerPassword = userNamePassword[1];
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets referer HTTP header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Referer"/> is <see langword="null"/>.</para>
        /// </exception>
        [Category("HTTP"), Description("The value of referer HTTP header to send to remote server."), DefaultValue(HttpUrl.DefaultHttpReferer)]
        public String Referer
        {
            get { return this.referer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Referer");
                }

                this.referer = value;
            }
        }

        /// <summary>
        /// Gets or sets user agent HTTP header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="UserAgent"/> is <see langword="null"/>.</para>
        /// </exception>
        [Category("HTTP"), Description("The value of user agent HTTP header to send to remote server."), DefaultValue(HttpUrl.DefaultHttpUserAgent)]
        public String UserAgent
        {
            get { return this.userAgent; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("UserAgent");
                }

                this.userAgent = value;
            }
        }

        /// <summary>
        /// Gets or sets HTTP version.
        /// </summary>
        /// <remarks>
        /// If <see cref="Version"/> is <see langword="null"/>, than version supported by remote server is used.
        /// </remarks>
        [Category("HTTP"), Description("Forces to use specific HTTP protocol version. "), DefaultValue(HttpUrl.DefaultHttpVersion)]
        [TypeConverter(typeof(HttpVersionConverter))]
        public Version Version
        {
            get { return this.version; }
            set
            {
                this.version = value;
            }
        }

        /// <summary>
        /// Gets or sets ignore content length flag.
        /// </summary>
        /// <remarks>
        /// This is useful to set for Apache 1.x (and similar servers) which will report incorrect content length for files over 2 gigabytes.
        /// </remarks>
        [Category("HTTP"), Description("Specifies if content length HTTP header have to be ignored (e.g. because server reports bad content length)."), DefaultValue(HttpUrl.DefaultHttpIgnoreContentLength)]
        public Boolean IgnoreContentLength { get; set; }

        /// <summary>
        /// Gets or sets HTTP cookie header.
        /// </summary>
        [Category("HTTP"), Description("The HTTP cookie header.")]
        public String Cookie
        {
            get { return this.cookie; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Cookie");
                }

                this.cookie = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout to open HTTP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("Connection"), Description("The timeout to open url in milliseconds."), DefaultValue(HttpUrl.DefaultHttpOpenConnectionTimeout)]
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
        [Category("Connection"), Description("The time in milliseconds to sleep before opening connection."), DefaultValue(HttpUrl.DefaultHttpOpenConnectionSleepTime)]
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
        [Category("Connection"), Description("The total timeout to open url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored."), DefaultValue(HttpUrl.DefaultHttpTotalReopenConnectionTimeout)]
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
        /// Specifies if seeking is supported by specifying range HTTP header in request.
        /// </summary>
        [Category("HTTP"), Description("Specifies if seeking is supported by specifying range HTTP header in request."), DefaultValue(HttpUrl.DefaultHttpSeekingSupported)]
        public Boolean SeekingSupported { get; set; }

        /// <summary>
        /// Enables or disables automatic detection of seeking support.
        /// </summary>
        [Category("HTTP"), Description("Enables or disables automatic detection of seeking support."), DefaultValue(HttpUrl.DefaultHttpSeekingSupportDetection)]
        public Boolean SeekingSupportDetection { get; set; }

        /// <summary>
        /// Specifies if filter has to authenticate against remote server.
        /// </summary>
        [Category("Server authentication"), Description("Specifies if filter has to authenticate against remote server."), DefaultValue(HttpUrl.DefaultHttpServerAuthenticate)]
        public Boolean ServerAuthenticate { get; set; }

        /// <summary>
        /// Gets or sets the remote server user name.
        /// </summary>
        [Category("Server authentication"), Description("The remote server user name."), DefaultValue(HttpUrl.DefaultHttpServerUserName)]
        public String ServerUserName
        {
            get { return this.serverUserName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ServerUserName");
                }

                this.serverUserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the remote server password.
        /// </summary>
        [Category("Server authentication"), Description("The remote server password."), DefaultValue(HttpUrl.DefaultHttpServerPassword)]
        public String ServerPassword
        {
            get { return this.serverPassword; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ServerPassword");
                }

                this.serverPassword = value;
            }
        }

        /// <summary>
        /// Specifies if filter has to authenticate against proxy server.
        /// </summary>
        [Category("Proxy server authentication"), Description("Specifies if filter has to authenticate against proxy server."), DefaultValue(HttpUrl.DefaultHttpProxyServerAuthenticate)]
        public Boolean ProxyServerAuthenticate { get; set; }

        /// <summary>
        /// Gets or sets the proxy server.
        /// </summary>
        [Category("Proxy server authentication"), Description("The proxy server."), DefaultValue(HttpUrl.DefaultHttpProxyServer)]
        public String ProxyServer
        {
            get { return this.proxyServer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServer");
                }

                this.proxyServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server port.
        /// </summary>
        [Category("Proxy server authentication"), Description("The proxy server port."), DefaultValue(HttpUrl.DefaultHttpProxyServerPort)]
        public int ProxyServerPort
        {
            get { return this.proxyServerPort; }
            set
            {
                if ((value < 0) || (value > 65535))
                {
                    throw new ArgumentOutOfRangeException("ProxyServerPort", value, "Must be greater than or equal to zero and lower than 65536.");
                }

                this.proxyServerPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server user name.
        /// </summary>
        [Category("Proxy server authentication"), Description("The proxy server user name."), DefaultValue(HttpUrl.DefaultHttpProxyServerUserName)]
        public String ProxyServerUserName
        {
            get { return this.proxyServerUserName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServerUserName");
                }

                this.proxyServerUserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server password.
        /// </summary>
        [Category("Proxy server authentication"), Description("The proxy server password."), DefaultValue(HttpUrl.DefaultHttpProxyServerPassword)]
        public String ProxyServerPassword
        {
            get { return this.proxyServerPassword; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServerPassword");
                }

                this.proxyServerPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server type.
        /// </summary>
        [Category("Proxy server authentication"), Description("The proxy server type."), DefaultValue(HttpUrl.DefaultHttpProxyServerType)]
        public ProxyServerType ProxyServerType
        {
            get { return this.proxyServerType; }
            set
            {
                switch (value)
                {
                    case ProxyServerType.None:
                    case ProxyServerType.HTTP:
                    case ProxyServerType.HTTP_1_0:
                    case ProxyServerType.SOCKS4:
                    case ProxyServerType.SOCKS5:
                    case ProxyServerType.SOCKS4A:
                    case ProxyServerType.SOCKS5_HOSTNAME:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("ProxyServerType", value, "The proxy server type value is unknown.");
                }

                this.proxyServerType = value;
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

            if (this.IgnoreContentLength != HttpUrl.DefaultHttpIgnoreContentLength)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpIgnoreContentLength, this.IgnoreContentLength ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }
            if (this.OpenConnectionTimeout != HttpUrl.DefaultHttpOpenConnectionTimeout)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpOpenConnectionTimeout, this.OpenConnectionTimeout.ToString()));
            }
            if (this.OpenConnectionSleepTime != HttpUrl.DefaultHttpOpenConnectionSleepTime)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpOpenConnectionSleepTime, this.OpenConnectionSleepTime.ToString()));
            }
            if (this.TotalReopenConnectionTimeout != HttpUrl.DefaultHttpTotalReopenConnectionTimeout)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpTotalReopenConnectionTimeout, this.TotalReopenConnectionTimeout.ToString()));
            }
            if (String.CompareOrdinal(this.Referer, HttpUrl.DefaultHttpReferer) != 0)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpReferer, this.Referer.ToString()));
            }
            if (String.CompareOrdinal(this.UserAgent, HttpUrl.DefaultHttpUserAgent) != 0)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpUserAgent, this.UserAgent.ToString()));
            }

            if (this.Version == HttpVersion.Version10)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpVersion, HttpUrl.HttpVersionForce10.ToString()));
            }
            else if (this.Version == HttpVersion.Version11)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpVersion, HttpUrl.HttpVersionForce11.ToString()));
            }

            if (String.CompareOrdinal(this.Cookie, HttpUrl.DefaultHttpCookie) != 0)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpCookie, this.Cookie));
            }

            if (this.SeekingSupported != HttpUrl.DefaultHttpSeekingSupported)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpSeekingSupported, this.SeekingSupported ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            if (this.SeekingSupportDetection != HttpUrl.DefaultHttpSeekingSupportDetection)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpSeekingSupportDetection, this.SeekingSupportDetection ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
            }

            if (this.ServerAuthenticate != HttpUrl.DefaultHttpServerAuthenticate)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpServerAuthenticate, this.ServerAuthenticate ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpServerUserName, this.ServerUserName));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpServerPassword, this.ServerPassword));
            }

            if (this.ProxyServerAuthenticate != HttpUrl.DefaultHttpProxyServerAuthenticate)
            {
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServerAuthenticate, this.ProxyServerAuthenticate ? SimpleUrl.DefaultTrue : SimpleUrl.DefaultFalse));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServer, this.ProxyServer));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServerPort, this.ProxyServerPort.ToString()));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServerUserName, this.ProxyServerUserName));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServerPassword, this.ProxyServerPassword));
                parameters.Add(new Parameter(HttpUrl.ParameterHttpProxyServerType, ((int)this.ProxyServerType).ToString()));
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
                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpOpenConnectionTimeout) == 0)
                {
                    this.OpenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpOpenConnectionSleepTime) == 0)
                {
                    this.OpenConnectionSleepTime = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpTotalReopenConnectionTimeout) == 0)
                {
                    this.TotalReopenConnectionTimeout = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpReferer) == 0)
                {
                    this.Referer = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpUserAgent) == 0)
                {
                    this.UserAgent = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpCookie) == 0)
                {
                    this.Cookie = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpVersion) == 0)
                {
                    if (String.CompareOrdinal(HttpUrl.HttpVersionForce10.ToString(), param.Value) == 0)
                    {
                        this.Version = HttpVersion.Version10;
                    }
                    else if (String.CompareOrdinal(HttpUrl.HttpVersionForce11.ToString(), param.Value) == 0)
                    {
                        this.Version = HttpVersion.Version11;
                    }
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpIgnoreContentLength) == 0)
                {
                    this.IgnoreContentLength = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpSeekingSupported) == 0)
                {
                    this.SeekingSupported = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpSeekingSupportDetection) == 0)
                {
                    this.SeekingSupportDetection = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                /* server authentication */

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpServerAuthenticate) == 0)
                {
                    this.ServerAuthenticate = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpServerUserName) == 0)
                {
                    this.ServerUserName = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpServerPassword) == 0)
                {
                    this.ServerPassword = param.Value;
                }

                /* proxy server authentication */

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServerAuthenticate) == 0)
                {
                    this.ProxyServerAuthenticate = (String.CompareOrdinal(param.Value, SimpleUrl.DefaultTrue) == 0);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServer) == 0)
                {
                    this.ProxyServer = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServerPort) == 0)
                {
                    this.ProxyServerPort = int.Parse(param.Value);
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServerUserName) == 0)
                {
                    this.ProxyServerUserName = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServerPassword) == 0)
                {
                    this.ProxyServerPassword = param.Value;
                }

                if (String.CompareOrdinal(param.Name, HttpUrl.ParameterHttpProxyServerType) == 0)
                {
                    this.ProxyServerType = (ProxyServerType)int.Parse(param.Value);
                }
            }
        }

        public override void ApplyDefaultUserSettings(ProtocolSettings previousSettings, ProtocolSettings currentSettings)
        {
            base.ApplyDefaultUserSettings(previousSettings, currentSettings);

            HttpProtocolSettings httpPreviousSettings = (HttpProtocolSettings)previousSettings;
            HttpProtocolSettings httpCurrentSettings = (HttpProtocolSettings)currentSettings;

            if ((this.OpenConnectionTimeout == HttpUrl.DefaultHttpOpenConnectionTimeout) ||
                (this.OpenConnectionTimeout == httpPreviousSettings.OpenConnectionTimeout))
            {
                this.OpenConnectionTimeout = httpCurrentSettings.OpenConnectionTimeout;
            }

            if ((this.OpenConnectionSleepTime == HttpUrl.DefaultHttpOpenConnectionSleepTime) ||
                (this.OpenConnectionSleepTime == httpPreviousSettings.OpenConnectionSleepTime))
            {
                this.OpenConnectionSleepTime = httpCurrentSettings.OpenConnectionSleepTime;
            }

            if ((this.TotalReopenConnectionTimeout == HttpUrl.DefaultHttpTotalReopenConnectionTimeout) ||
                (this.TotalReopenConnectionTimeout == httpPreviousSettings.TotalReopenConnectionTimeout))
            {
                this.TotalReopenConnectionTimeout = httpCurrentSettings.TotalReopenConnectionTimeout;
            }

            /* server authentication */

            if ((this.ServerAuthenticate == HttpUrl.DefaultHttpServerAuthenticate) ||
                (this.ServerAuthenticate == httpPreviousSettings.EnableServerAuthentication))
            {
                this.ServerAuthenticate = httpCurrentSettings.EnableServerAuthentication;
            }

            if ((this.ServerUserName == HttpUrl.DefaultHttpServerUserName) ||
                (this.ServerUserName == httpPreviousSettings.ServerUserName))
            {
                this.ServerUserName = httpCurrentSettings.ServerUserName;
            }

            if ((this.ServerPassword == HttpUrl.DefaultHttpServerPassword) ||
                (this.ServerPassword == httpPreviousSettings.ServerPassword))
            {
                this.ServerPassword = httpCurrentSettings.ServerPassword;
            }

            /* proxy server authentication */

            if ((this.ProxyServerAuthenticate == HttpUrl.DefaultHttpProxyServerAuthenticate) ||
                (this.ProxyServerAuthenticate == httpPreviousSettings.EnableProxyServerAuthentication))
            {
                this.ProxyServerAuthenticate = httpCurrentSettings.EnableProxyServerAuthentication;
            }

            if ((this.ProxyServer == HttpUrl.DefaultHttpProxyServer) ||
                (this.ProxyServer == httpPreviousSettings.ProxyServer))
            {
                this.ProxyServer = httpCurrentSettings.ProxyServer;
            }

            if ((this.ProxyServerPort == HttpUrl.DefaultHttpProxyServerPort) ||
                (this.ProxyServerPort == httpPreviousSettings.ProxyServerPort))
            {
                this.ProxyServerPort = httpCurrentSettings.ProxyServerPort;
            }

            if ((this.ProxyServerUserName == HttpUrl.DefaultHttpProxyServerUserName) ||
                (this.ProxyServerUserName == httpPreviousSettings.ProxyServerUserName))
            {
                this.ProxyServerUserName = httpCurrentSettings.ProxyServerUserName;
            }

            if ((this.ProxyServerPassword == HttpUrl.DefaultHttpProxyServerPassword) ||
                (this.ProxyServerPassword == httpPreviousSettings.ProxyServerPassword))
            {
                this.ProxyServerPassword = httpCurrentSettings.ProxyServerPassword;
            }

            if ((this.ProxyServerType == HttpUrl.DefaultHttpProxyServerType) ||
                (this.ProxyServerType == httpPreviousSettings.ProxyServerType))
            {
                this.ProxyServerType = httpCurrentSettings.ProxyServerType;
            }
        }

        #endregion

        #region Constants

        /* parameters */

        /// <summary>
        /// Specifies open connection timeout in milliseconds.
        /// </summary>
        protected static readonly String ParameterHttpOpenConnectionTimeout = "HttpOpenConnectionTimeout";

        /// <summary>
        /// Specifies the time in milliseconds to sleep before opening connection.
        /// </summary>
        protected static readonly String ParameterHttpOpenConnectionSleepTime = "HttpOpenConnectionSleepTime";

        /// <summary>
        /// Specifies the total timeout to open HTTP url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.
        /// </summary>
        protected static readonly String ParameterHttpTotalReopenConnectionTimeout = "HttpTotalReopenConnectionTimeout";

        /// <summary>
        /// Specifies the value of referer HTTP header to send to remote server.
        /// </summary>
        protected static readonly String ParameterHttpReferer = "HttpReferer";

        /// <summary>
        /// Specifies the value of user agent HTTP header to send to remote server.
        /// </summary>
        protected static readonly String ParameterHttpUserAgent = "HttpUserAgent";

        /// <summary>
        /// Specifies the value of cookie HTTP header to send to remote server.
        /// </summary>
        protected static readonly String ParameterHttpCookie = "HttpCookie";

        /// <summary>
        /// Forces to use specific HTTP protocol version
        /// </summary>
        protected static readonly String ParameterHttpVersion = "HttpVersion";

        /// <summary>
        /// Specifies that version of HTTP protocol is not specified.
        /// </summary>
        protected static readonly int HttpVersionNone = 0;

        /// <summary>
        /// Forces to use HTTP version 1.0.
        /// </summary>
        protected static readonly int HttpVersionForce10 = 1;

        /// <summary>
        /// Forces to use HTTP version 1.1.
        /// </summary>
        protected static readonly int HttpVersionForce11 = 2;

        /// <summary>
        /// Specifies if content length HTTP header have to be ignored (e.g. because server reports bad content length).
        /// </summary>
        protected static readonly String ParameterHttpIgnoreContentLength = "HttpIgnoreContentLength";

        /// <summary>
        /// Specifies if seeking is supported by specifying range HTTP header in request.
        /// </summary>
        protected static readonly String ParameterHttpSeekingSupported = "HttpSeekingSupported";

        /// <summary>
        /// Enables or disables automatic detection of seeking support.
        /// </summary>
        protected static readonly String ParameterHttpSeekingSupportDetection = "HttpSeekingSupportDetection";

        /// <summary>
        /// Specifies if filter has to authenticate against remote server.
        /// </summary>
        protected static readonly String ParameterHttpServerAuthenticate = "HttpServerAuthenticate";

        /// <summary>
        /// Specifies the value of remote server user name to authenticate.
        /// </summary>
        protected static readonly String ParameterHttpServerUserName = "HttpServerUserName";

        /// <summary>
        /// Specifies the value of remote server password to authenticate.
        /// </summary>
        protected static readonly String ParameterHttpServerPassword = "HttpServerPassword";

        /// <summary>
        /// Specifies if filter has to authenticate against proxy server.
        /// </summary>
        protected static readonly String ParameterHttpProxyServerAuthenticate = "HttpProxyServerAuthenticate";

        /// <summary>
        /// Specifies the value of proxy server.
        /// </summary>
        protected static readonly String ParameterHttpProxyServer = "HttpProxyServer";

        /// <summary>
        /// Specifies the value of proxy server port.
        /// </summary>
        protected static readonly String ParameterHttpProxyServerPort = "HttpProxyServerPort";

        /// <summary>
        /// Specifies the value of remote server user name to authenticate.
        /// </summary>
        protected static readonly String ParameterHttpProxyServerUserName = "HttpProxyServerUserName";

        /// <summary>
        /// Specifies the value of remote server password to authenticate.
        /// </summary>
        protected static readonly String ParameterHttpProxyServerPassword = "HttpProxyServerPassword";

        /// <summary>
        /// Specifies the value of proxy server type.
        /// </summary>
        protected static readonly String ParameterHttpProxyServerType = "HttpProxyServerType";

        /* default values */

        /// <summary>
        /// Default value for <see cref="ParameterHttpOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultHttpOpenConnectionTimeout = 5000;

        /// <summary>
        /// Default value for <see cref="ParameterHttpOpenConnectionSleepTime"/>.
        /// </summary>
        public const int DefaultHttpOpenConnectionSleepTime = 0;

        /// <summary>
        /// Default value for <see cref="ParameterHttpTotalReopenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultHttpTotalReopenConnectionTimeout = 60000;

        /// <summary>
        /// Default value for <see cref="ParameterHttpReferer"/>.
        /// </summary>
        public const String DefaultHttpReferer = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpUserAgent"/>.
        /// </summary>
        public const String DefaultHttpUserAgent = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpCookie"/>.
        /// </summary>
        public static readonly String DefaultHttpCookie = String.Empty;

        /// <summary>
        /// Default value for <see cref="ParameterHttpVersion"/>.
        /// </summary>
        public const Version DefaultHttpVersion = null;

        /// <summary>
        /// Default value for <see cref="ParameterHttpIgnoreContentLength"/>.
        /// </summary>
        public const Boolean DefaultHttpIgnoreContentLength = false;

        /// <summary>
        /// Default value for <see cref="ParameterHttpSeekingSupported"/>.
        /// </summary>
        public const Boolean DefaultHttpSeekingSupported = false;

        /// <summary>
        /// Default value for <see cref="ParameterHttpSeekingSupportDetection"/>.
        /// </summary>
        public const Boolean DefaultHttpSeekingSupportDetection = true;

        /// <summary>
        /// Default value for <see cref="ParameterHttpServerAuthenticate"/>.
        /// </summary>
        public const Boolean DefaultHttpServerAuthenticate = false;

        /// <summary>
        /// Default value for <see cref="ParameterHttpServerUserName"/>.
        /// </summary>
        public const String DefaultHttpServerUserName = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpServerPassword"/>.
        /// </summary>
        public const String DefaultHttpServerPassword = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServerAuthenticate"/>.
        /// </summary>
        public const Boolean DefaultHttpProxyServerAuthenticate = false;

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServer"/>.
        /// </summary>
        public const String DefaultHttpProxyServer = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServerPort"/>.
        /// </summary>
        public const int DefaultHttpProxyServerPort = 1080;

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServerUserName"/>.
        /// </summary>
        public const String DefaultHttpProxyServerUserName = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServerPassword"/>.
        /// </summary>
        public const String DefaultHttpProxyServerPassword = "";

        /// <summary>
        /// Default value for <see cref="ParameterHttpProxyServerType"/>.
        /// </summary>
        public const ProxyServerType DefaultHttpProxyServerType = ProxyServerType.HTTP;

        #endregion
    }
}
