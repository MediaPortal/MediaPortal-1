using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Collections;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents settings class for MediaPortal IPTV filter and url source splitter.
    /// </summary>
    [XmlRoot("Settings")]
    public class Settings
    {
        #region Private field
        #endregion

        #region Constructors

        static Settings()
        {
            Settings.SupportedProtocols = new Hashtable();

            Settings.SupportedProtocols.Add("HTTP", "HTTP");
            Settings.SupportedProtocols.Add("HTTPS", "HTTP");

            Settings.SupportedProtocols.Add("RTMP", "RTMP");
            Settings.SupportedProtocols.Add("RTMPT", "RTMP");
            Settings.SupportedProtocols.Add("RTMPE", "RTMP");
            Settings.SupportedProtocols.Add("RTMPTE", "RTMP");
            Settings.SupportedProtocols.Add("RTMPS", "RTMP");
            Settings.SupportedProtocols.Add("RTMPTS", "RTMP");

            Settings.SupportedProtocols.Add("RTSP", "RTSP");

            Settings.SupportedProtocols.Add("UDP", "UDP");
            Settings.SupportedProtocols.Add("RTP", "UDP");
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            this.Http = new HttpProtocolSettings();
            this.Rtmp = new RtmpProtocolSettings();
            this.Rtsp = new RtspProtocolSettings();
            this.UdpRtp = new UdpRtpProtocolSettings();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets HTTP protocol settings.
        /// </summary>
        public HttpProtocolSettings Http { get; set; }

        /// <summary>
        /// Gets or sets RTMP protocol settings.
        /// </summary>
        public RtmpProtocolSettings Rtmp { get; set; }

        /// <summary>
        /// Gets or sets RTSP protocol settings.
        /// </summary>
        public RtspProtocolSettings Rtsp { get; set; }

        /// <summary>
        /// Gets or sets UDP or RTP protocol settings.
        /// </summary>
        public UdpRtpProtocolSettings UdpRtp { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads settings from configuration file.
        /// </summary>
        /// <returns>An instance of <see cref="Settings"/> class.</returns>
        public static Settings Load()
        {
            String configFilePath = Path.Combine(TvLibrary.Interfaces.PathManager.GetDataPath, Settings.ConfigurationFileName);

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
            {
                return (Settings)serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Saves specified settings into configuration file.
        /// </summary>
        /// <param name="settings">The settings to save into configuration file.</param>
        public static void Save(Settings settings)
        {
            String configFilePath = Path.Combine(TvLibrary.Interfaces.PathManager.GetDataPath, Settings.ConfigurationFileName);

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream stream = new FileStream(configFilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                serializer.Serialize(stream, settings);
            }
        }

        #endregion

        #region Constants

        public static readonly String ConfigurationFileName = "MediaPortalIptvFilterAndUrlSourceSplitter.xml";

        public static readonly Hashtable SupportedProtocols = null;

        #endregion
    }
}
