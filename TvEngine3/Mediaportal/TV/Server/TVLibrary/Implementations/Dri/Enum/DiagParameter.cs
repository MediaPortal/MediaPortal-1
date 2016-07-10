#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum
{
  internal sealed class DiagParameter
  {
    private readonly string _name;
    private readonly TunerVendor _supportedVendors;
    private static readonly IDictionary<string, DiagParameter> _values = new Dictionary<string, DiagParameter>();

    #region standard values - specified in OC-SP-DRI-I04

    /// <summary>
    /// Name of the DRIT manufacturer.
    /// </summary>
    public static readonly DiagParameter HostManufacturer = new DiagParameter("Host Manufacturer");
    /// <summary>
    /// Serial Number of the DRIT.
    /// </summary>
    public static readonly DiagParameter HostSerialNumber = new DiagParameter("Host Serial Number");
    /// <summary>
    /// Unique ID of the DRIT used for Card/Host binding.
    /// </summary>
    public static readonly DiagParameter HostId = new DiagParameter("Host ID");
    /// <summary>
    /// Explicit description of the current power status.
    /// </summary>
    public static readonly DiagParameter HostPowerStatus = new DiagParameter("Host Power Status");
    /// <summary>
    /// Explicit description of the current boot status.
    /// </summary>
    public static readonly DiagParameter HostBootStatus = new DiagParameter("Host Boot Status");
    /// <summary>
    /// Explicit description of the current memory allocation.
    /// </summary>
    public static readonly DiagParameter HostMemoryReport = new DiagParameter("Host Memory Report");
    /// <summary>
    /// Explicit description of the DRM application supported by the devices, including name, version number and date.
    /// </summary>
    public static readonly DiagParameter HostApplication = new DiagParameter("Host Application");
    /// <summary>
    /// Explicit description of the DRIT firmware including name, version number and date.
    /// </summary>
    public static readonly DiagParameter HostFirmware = new DiagParameter("Host Firmware");

    #endregion

    #region de-facto standard values - not specified in OC-SP-DRI-I04, but supported by all current tuner vendors

    public static readonly DiagParameter HostConnection = new DiagParameter("Host Connection");
    public static readonly DiagParameter HostIpAddress = new DiagParameter("Host IP Address");
    public static readonly DiagParameter HostMacAddress = new DiagParameter("Host MAC Address");
    public static readonly DiagParameter SignalChannel = new DiagParameter("Signal Channel");
    public static readonly DiagParameter SignalChannelFormat = new DiagParameter("Signal Channel Format");
    public static readonly DiagParameter SignalFrequency = new DiagParameter("Signal Frequency");
    public static readonly DiagParameter SignalModulation = new DiagParameter("Signal Modulation");
    public static readonly DiagParameter SignalCarrierLock = new DiagParameter("Signal Carrier Lock");
    public static readonly DiagParameter SignalPcrLock = new DiagParameter("Signal PCR Lock");
    public static readonly DiagParameter SignalSnr = new DiagParameter("Signal SNR");
    public static readonly DiagParameter SignalLevel = new DiagParameter("Signal Level");
    public static readonly DiagParameter SignalAuthorizationStatus = new DiagParameter("Signal Authorization Status");
    public static readonly DiagParameter SignalPurchasableStatus = new DiagParameter("Signal Purchasable Status");
    public static readonly DiagParameter SignalPurchasedStatus = new DiagParameter("Signal Purchased Status");
    public static readonly DiagParameter SignalPreviewStatus = new DiagParameter("Signal Preview Status");
    public static readonly DiagParameter OobStatus = new DiagParameter("OOB Status");
    public static readonly DiagParameter OobCenterFrequency = new DiagParameter("OOB Center Frequency");
    public static readonly DiagParameter OobBitRate = new DiagParameter("OOB Bit Rate");
    public static readonly DiagParameter CableCardStatus = new DiagParameter("CableCard Status");
    public static readonly DiagParameter CableCardManufacturer = new DiagParameter("CableCard Manufacturer");
    public static readonly DiagParameter CableCardVersion = new DiagParameter("CableCard Version");

    #endregion

    #region proprietary values

    public static readonly DiagParameter CvtImageFilename = new DiagParameter("CVT Image Filename", TunerVendor.Ati | TunerVendor.Ceton);
    public static readonly DiagParameter CableCardId = new DiagParameter("CableCard ID", TunerVendor.Hauppauge | TunerVendor.SiliconDust);

    public static readonly DiagParameter HostStorageReport = new DiagParameter("Host Storage Report", TunerVendor.Ceton);
    public static readonly DiagParameter HardwareRevision = new DiagParameter("Hardware Revision", TunerVendor.Ceton);
    public static readonly DiagParameter Temperature = new DiagParameter("Temperature", TunerVendor.Ceton);
    public static readonly DiagParameter OobSignalLevel = new DiagParameter("OOB Signal Level", TunerVendor.Ceton);
    public static readonly DiagParameter OobSnr = new DiagParameter("OOB SNR", TunerVendor.Ceton);
    public static readonly DiagParameter ImageReleaseDate = new DiagParameter("Image Release Date", TunerVendor.Ceton);
    public static readonly DiagParameter ImageInstallDate = new DiagParameter("Image Install Date", TunerVendor.Ceton);
    public static readonly DiagParameter Uptime = new DiagParameter("Uptime", TunerVendor.Ceton);
    public static readonly DiagParameter CopyProtectionStatus = new DiagParameter("Copy Protection Status", TunerVendor.Ceton);
    public static readonly DiagParameter NumChannels = new DiagParameter("Number of Channels", TunerVendor.Ceton);
    public static readonly DiagParameter NumMapsRecv = new DiagParameter("Channel Maps Received", TunerVendor.Ceton);
    public static readonly DiagParameter StaticIp = new DiagParameter("Static IP", TunerVendor.Ceton);
    public static readonly DiagParameter Netmask = new DiagParameter("Netmask", TunerVendor.Ceton);
    public static readonly DiagParameter DhcpClient = new DiagParameter("DHCP Client", TunerVendor.Ceton);
    public static readonly DiagParameter WmDrmStatus = new DiagParameter("WMDRM Status", TunerVendor.Ceton);
    public static readonly DiagParameter WmDrmVersion = new DiagParameter("WMDRM Version", TunerVendor.Ceton);

    #endregion

    private DiagParameter(string name, TunerVendor supportedVendors = TunerVendor.All)
    {
      _name = name;
      _supportedVendors = supportedVendors;
      _values.Add(name, this);
    }

    public TunerVendor SupportedVendors
    {
      get
      {
        return _supportedVendors;
      }
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DiagParameter diagParam = obj as DiagParameter;
      if (diagParam != null && this == diagParam)
      {
        return true;
      }
      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public static ICollection<DiagParameter> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator DiagParameter(string name)
    {
      DiagParameter value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DiagParameter diagParam)
    {
      return diagParam._name;
    }
  }
}