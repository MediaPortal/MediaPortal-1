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
        private bool ignoreContentLength = HttpUrl.DefaultHttpIgnoreContentLength;
        private int openConnectionTimeout = HttpUrl.DefaultHttpOpenConnectionTimeout;
        private int openConnectionSleepTime = HttpUrl.DefaultHttpOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = HttpUrl.DefaultHttpTotalReopenConnectionTimeout;
        private String cookie = HttpUrl.DefaultHttpCookie;

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
            this.SeekingSupported = HttpUrl.DefaultHttpSeekingSupported;
            this.SeekingSupportDetection = HttpUrl.DefaultHttpSeekingSupportDetection;
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
        public Boolean IgnoreContentLength
        {
            get { return this.ignoreContentLength; }
            set
            {
                this.ignoreContentLength = value;
            }
        }

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
            }
        }

        #endregion

        #region Constants

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
        /// Default value for <see cref="ParameterHttpOpenConnectionTimeout"/>.
        /// </summary>
        public const int DefaultHttpOpenConnectionTimeout = 20000;

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

        #endregion
    }
}
