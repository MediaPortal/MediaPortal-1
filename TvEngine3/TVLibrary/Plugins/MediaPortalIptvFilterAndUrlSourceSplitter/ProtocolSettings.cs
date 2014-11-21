using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter
{
    /// <summary>
    /// Represents base class for all protocols.
    /// </summary>
    [XmlRoot("ProtocolSettings")]
    public class ProtocolSettings
    {
        #region Private fields

        private String networkInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ProtocolSettings"/> class.
        /// </summary>
        public ProtocolSettings()
        {
            this.NetworkInterface = ProtocolSettings.NetworkInterfaceSystemDefault;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the network interface.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="NetworkInterface"/> is <see langword="null"/>.</para>
        /// </exception>
        public String NetworkInterface
        {
            get { return this.networkInterface; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NetworkInterface");
                }

                this.networkInterface = value;
            }
        }

        #endregion

        #region Methods
        #endregion

        #region Constants

        public static readonly String NetworkInterfaceSystemDefault = "System default";

        #endregion
    }
}
