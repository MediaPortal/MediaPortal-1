using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for creating URL objects from specified string.
    /// </summary>
    internal static class UrlFactory
    {
        #region Private fields
        #endregion

        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Methods

        public static SimpleUrl CreateUrl(String url)
        {
            ParameterCollection parameters = ParameterCollection.GetParameters(url);

            if (parameters.Count == 0)
            {
                // no special form of URL
                // in this case check URI scheme

                Uri uri = new Uri(url);
                String scheme = (String)Settings.SupportedProtocols[uri.Scheme.ToUpperInvariant()];

                switch (scheme)
                {
                    case "HTTP":
                        return new HttpUrl(url);
                    case "RTSP":
                        return new RtspUrl(url);
                    case "RTMP":
                        return new RtmpUrl(url);
                    case "UDP":
                        return new UdpRtpUrl(url);
                    default:
                        return null;
                }
            }
            else
            {
                // special form of URL
                // find URL parameter

                url = String.Empty;
                foreach (var param in parameters)
                {
                    if (String.CompareOrdinal(param.Name, SimpleUrl.ParameterUrl) == 0)
                    {
                        url = param.Value;
                        break;
                    }
                }

                Uri uri = new Uri(url);
                String scheme = (String)Settings.SupportedProtocols[uri.Scheme.ToUpperInvariant()];

                SimpleUrl result = null;

                switch (scheme)
                {
                    case "HTTP":
                        result = new HttpUrl(url);
                        break;
                    case "RTSP":
                        result = new RtspUrl(url);
                        break;
                    case "RTMP":
                        result = new RtmpUrl(url);
                        break;
                    case "UDP":
                        result = new UdpRtpUrl(url);
                        break;
                }

                result.Parse(parameters);

                return result;
            }
        }

        #endregion
    }
}
