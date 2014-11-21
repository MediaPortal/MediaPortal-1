using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents class for network interface converter.
    /// </summary>
    public class NetworkInterfaceConverter : StringConverter
    {
        #region Methods

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<String> nics = new List<String>();
            System.Net.NetworkInformation.NetworkInterface[] networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            nics.Add(NetworkInterfaceConverter.NetworkInterfaceSystemDefault);
            foreach (var networkInterface in networkInterfaces)
            {
                nics.Add(networkInterface.Name);
            }

            return new StandardValuesCollection(nics);
        }

        #endregion

        #region Constants

        public const String NetworkInterfaceSystemDefault = "System default";

        #endregion
    }

}
