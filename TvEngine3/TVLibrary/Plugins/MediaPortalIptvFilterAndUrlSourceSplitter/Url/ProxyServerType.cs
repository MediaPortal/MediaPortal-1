using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents proxy server type.
    /// </summary>
    public enum ProxyServerType
    {
        None = 0,

        HTTP = 1,

        HTTP_1_0 = 2,

        SOCKS4 = 3,

        SOCKS5 = 4,

        SOCKS4A = 5,

        SOCKS5_HOSTNAME = 6
    }
}
