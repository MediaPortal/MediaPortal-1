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
  internal class DiagParameterCeton
  {
    private readonly string _name;
    private static readonly IDictionary<string, DiagParameterCeton> _values = new Dictionary<string, DiagParameterCeton>();

    public static readonly DiagParameterCeton HostStorageReport = new DiagParameterCeton("Host Storage Report");
    public static readonly DiagParameterCeton HardwareRevision = new DiagParameterCeton("Hardware Revision");
    public static readonly DiagParameterCeton HostConnection = new DiagParameterCeton("Host Connection");
    public static readonly DiagParameterCeton HostIpAddress = new DiagParameterCeton("Host IP Address");
    public static readonly DiagParameterCeton HostMacAddress = new DiagParameterCeton("Host MAC Address");
    public static readonly DiagParameterCeton SignalChannel = new DiagParameterCeton("Signal Channel");
    public static readonly DiagParameterCeton SignalChannelFormat = new DiagParameterCeton("Signal Channel Format");
    public static readonly DiagParameterCeton SignalFrequency = new DiagParameterCeton("Signal Frequency");
    public static readonly DiagParameterCeton SignalModulation = new DiagParameterCeton("Signal Modulation");
    public static readonly DiagParameterCeton SignalCarrierLock = new DiagParameterCeton("Signal Carrier Lock");
    public static readonly DiagParameterCeton SignalPcrLock = new DiagParameterCeton("Signal PCR Lock");
    public static readonly DiagParameterCeton SignalSnr = new DiagParameterCeton("Signal SNR");
    public static readonly DiagParameterCeton SignalLevel = new DiagParameterCeton("Signal Level");
    public static readonly DiagParameterCeton Temperature = new DiagParameterCeton("Temperature");
    public static readonly DiagParameterCeton SignalAuthorizationStatus = new DiagParameterCeton("Signal Authorization Status");
    public static readonly DiagParameterCeton SignalPurchasableStatus = new DiagParameterCeton("Signal Purchasable Status");
    public static readonly DiagParameterCeton SignalPurchasedStatus = new DiagParameterCeton("Signal Purchased Status");
    public static readonly DiagParameterCeton SignalPreviewStatus = new DiagParameterCeton("Signal Preview Status");
    public static readonly DiagParameterCeton OobStatus = new DiagParameterCeton("OOB Status");
    public static readonly DiagParameterCeton OobCenterFrequency = new DiagParameterCeton("OOB Center Frequency");
    public static readonly DiagParameterCeton OobBitRate = new DiagParameterCeton("OOB Bit Rate");
    public static readonly DiagParameterCeton OobSignalLevel = new DiagParameterCeton("OOB Signal Level");
    public static readonly DiagParameterCeton OobSnr = new DiagParameterCeton("OOB SNR");
    public static readonly DiagParameterCeton CableCardStatus = new DiagParameterCeton("CableCard Status");
    public static readonly DiagParameterCeton CableCardManufacturer = new DiagParameterCeton("CableCard Manufacturer");
    public static readonly DiagParameterCeton CableCardVersion = new DiagParameterCeton("CableCard Version");
    public static readonly DiagParameterCeton ImageReleaseDate = new DiagParameterCeton("Image Release Date");
    public static readonly DiagParameterCeton ImageInstallDate = new DiagParameterCeton("Image Install Date");
    public static readonly DiagParameterCeton CvtImageFilename = new DiagParameterCeton("CVT Image Filename");
    public static readonly DiagParameterCeton Uptime = new DiagParameterCeton("Uptime");
    public static readonly DiagParameterCeton CopyProtectionStatus = new DiagParameterCeton("Copy Protection Status");
    public static readonly DiagParameterCeton NumChannels = new DiagParameterCeton("Number of Channels");
    public static readonly DiagParameterCeton NumMapsRecv = new DiagParameterCeton("Channel Maps Received");
    public static readonly DiagParameterCeton StaticIp = new DiagParameterCeton("Static IP");
    public static readonly DiagParameterCeton Netmask = new DiagParameterCeton("Netmask");
    public static readonly DiagParameterCeton DhcpClient = new DiagParameterCeton("DHCP Client");
    public static readonly DiagParameterCeton WmDrmStatus = new DiagParameterCeton("WMDRM Status");
    public static readonly DiagParameterCeton WmDrmVersion = new DiagParameterCeton("WMDRM Version");

    private DiagParameterCeton(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DiagParameterCeton diagParam = obj as DiagParameterCeton;
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

    public static ICollection<DiagParameterCeton> Values
    {
      get
      {
        return _values.Values;
      }
    }

    public static explicit operator DiagParameterCeton(string name)
    {
      DiagParameterCeton value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DiagParameterCeton diagParam)
    {
      return diagParam._name;
    }
  }
}