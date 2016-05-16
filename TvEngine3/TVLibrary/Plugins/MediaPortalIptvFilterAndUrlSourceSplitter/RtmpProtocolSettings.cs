using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents class for RTMP protocol settings.
    /// </summary>
    [XmlRoot("RtmpProtocolSettings")]
    public class RtmpProtocolSettings : ProtocolSettings
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtmpProtocolSettings"/> class.
        /// </summary>
        public RtmpProtocolSettings()
            : base()
        {
            this.OpenConnectionTimeout = RtmpUrl.DefaultRtmpOpenConnectionTimeout;
            this.OpenConnectionSleepTime = RtmpUrl.DefaultRtmpOpenConnectionSleepTime;
            this.TotalReopenConnectionTimeout = RtmpUrl.DefaultRtmpTotalReopenConnectionTimeout;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        #endregion
    }
}
