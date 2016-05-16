using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for HTTP protocol settings.
    /// </summary>
    [XmlRoot("HttpProtocolSettings")]
    public class HttpProtocolSettings : ProtocolSettings
    {
        #region Private fields

        /* server authentication */

        private String serverUserName;
        private String serverPassword;

        /* proxy server authentication */

        private String proxyServer;
        private int proxyServerPort;
        private String proxyServerUserName;
        private String proxyServerPassword;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="HttpProtocolSettings"/> class.
        /// </summary>
        public HttpProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = HttpUrl.DefaultHttpOpenConnectionTimeout;
            this.OpenConnectionSleepTime = HttpUrl.DefaultHttpOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = HttpUrl.DefaultHttpTotalReopenConnectionTimeout;

            this.EnableServerAuthentication = HttpUrl.DefaultHttpServerAuthenticate;
            this.ServerUserName = HttpUrl.DefaultHttpServerUserName;
            this.ServerPassword = HttpUrl.DefaultHttpServerPassword;

            this.EnableProxyServerAuthentication = HttpUrl.DefaultHttpProxyServerAuthenticate;
            this.ProxyServer = HttpUrl.DefaultHttpProxyServer;
            this.ProxyServerPort = HttpUrl.DefaultHttpProxyServerPort;
            this.ProxyServerUserName = HttpUrl.DefaultHttpProxyServerUserName;
            this.ProxyServerPassword = HttpUrl.DefaultHttpProxyServerPassword;
            this.ProxyServerType = HttpUrl.DefaultHttpProxyServerType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Specifies if server authentication is enabled.
        /// </summary>
        public Boolean EnableServerAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the server user name.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ServerUserName"/> is <see langword="null"/>.</para>
        /// </exception>
        public String ServerUserName
        {
            get { return this.serverUserName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ServerUserName", "Cannot be null.");
                }

                this.serverUserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the server password.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ServerUserName"/> is <see langword="null"/>.</para>
        /// </exception>
        public String ServerPassword
        {
            get { return this.serverPassword; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ServerPassword", "Cannot be null.");
                }

                this.serverPassword = value;
            }
        }

        /// <summary>
        /// Specifies if proxy server authentication is enabled.
        /// </summary>
        public Boolean EnableProxyServerAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the proxy server.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ProxyServer"/> is <see langword="null"/>.</para>
        /// </exception>
        public String ProxyServer
        {
            get { return this.proxyServer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServer", "Cannot be null.");
                }

                this.proxyServer = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server port.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ProxyServerPort"/> is less than zero or greater than 65535.</para>
        /// </exception>
        public int ProxyServerPort
        {
            get { return this.proxyServerPort; }
            set
            {
                if ((value < 0) || (value > 65535))
                {
                    throw new ArgumentOutOfRangeException("ProxyServerPort", value, "Cannot be less than zero or greater than 65535.");
                }

                this.proxyServerPort = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server user name.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ProxyServerUserName"/> is <see langword="null"/>.</para>
        /// </exception>
        public String ProxyServerUserName
        {
            get { return this.proxyServerUserName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServerUserName", "Cannot be null.");
                }

                this.proxyServerUserName = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server password.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="ProxyServerPassword"/> is <see langword="null"/>.</para>
        /// </exception>
        public String ProxyServerPassword
        {
            get { return this.proxyServerPassword; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ProxyServerPassword", "Cannot be null.");
                }

                this.proxyServerPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the proxy server type.
        /// </summary>
        public ProxyServerType ProxyServerType { get; set; }

        #endregion

        #region Methods
        #endregion
    }
}
