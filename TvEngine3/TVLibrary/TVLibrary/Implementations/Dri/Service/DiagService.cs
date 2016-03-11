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

using System;
using System.Collections.Generic;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public class CetonDiagParameter
  {
    private readonly string _name;
    private static readonly IDictionary<string, CetonDiagParameter> _values = new Dictionary<string, CetonDiagParameter>();

    public static readonly CetonDiagParameter HostStorageReport = new CetonDiagParameter("Host Storage Report");
    public static readonly CetonDiagParameter HardwareRevision = new CetonDiagParameter("Hardware Revision");
    public static readonly CetonDiagParameter HostConnection = new CetonDiagParameter("Host Connection");
    public static readonly CetonDiagParameter HostIpAddress = new CetonDiagParameter("Host IP Address");
    public static readonly CetonDiagParameter HostMacAddress = new CetonDiagParameter("Host MAC Address");
    public static readonly CetonDiagParameter SignalChannel = new CetonDiagParameter("Signal Channel");
	  public static readonly CetonDiagParameter SignalChannelFormat = new CetonDiagParameter("Signal Channel Format");
	  public static readonly CetonDiagParameter SignalFrequency = new CetonDiagParameter("Signal Frequency");
	  public static readonly CetonDiagParameter SignalModulation = new CetonDiagParameter("Signal Modulation");
	  public static readonly CetonDiagParameter SignalCarrierLock = new CetonDiagParameter("Signal Carrier Lock");
	  public static readonly CetonDiagParameter SignalPcrLock = new CetonDiagParameter("Signal PCR Lock");
	  public static readonly CetonDiagParameter SignalSnr = new CetonDiagParameter("Signal SNR");
	  public static readonly CetonDiagParameter SignalLevel = new CetonDiagParameter("Signal Level");
	  public static readonly CetonDiagParameter Temperature = new CetonDiagParameter("Temperature");
	  public static readonly CetonDiagParameter SignalAuthorizationStatus = new CetonDiagParameter("Signal Authorization Status");
	  public static readonly CetonDiagParameter SignalPurchasableStatus = new CetonDiagParameter("Signal Purchasable Status");
	  public static readonly CetonDiagParameter SignalPurchasedStatus = new CetonDiagParameter("Signal Purchased Status");
	  public static readonly CetonDiagParameter SignalPreviewStatus = new CetonDiagParameter("Signal Preview Status");
	  public static readonly CetonDiagParameter OobStatus = new CetonDiagParameter("OOB Status");
	  public static readonly CetonDiagParameter OobCenterFrequency = new CetonDiagParameter("OOB Center Frequency");
	  public static readonly CetonDiagParameter OobBitRate = new CetonDiagParameter("OOB Bit Rate");
	  public static readonly CetonDiagParameter OobSignalLevel = new CetonDiagParameter("OOB Signal Level");
	  public static readonly CetonDiagParameter OobSnr = new CetonDiagParameter("OOB SNR");
	  public static readonly CetonDiagParameter CableCardStatus = new CetonDiagParameter("CableCard Status");
	  public static readonly CetonDiagParameter CableCardManufacturer = new CetonDiagParameter("CableCard Manufacturer");
	  public static readonly CetonDiagParameter CableCardVersion = new CetonDiagParameter("CableCard Version");
	  public static readonly CetonDiagParameter ImageReleaseDate = new CetonDiagParameter("Image Release Date");
	  public static readonly CetonDiagParameter ImageInstallDate = new CetonDiagParameter("Image Install Date");
	  public static readonly CetonDiagParameter CvtImageFilename = new CetonDiagParameter("CVT Image Filename");
	  public static readonly CetonDiagParameter Uptime = new CetonDiagParameter("Uptime");
	  public static readonly CetonDiagParameter CopyProtectionStatus = new CetonDiagParameter("Copy Protection Status");
    public static readonly CetonDiagParameter NumChannels = new CetonDiagParameter("Number of Channels");
    public static readonly CetonDiagParameter NumMapsRecv = new CetonDiagParameter("Channel Maps Received");
    public static readonly CetonDiagParameter StaticIp = new CetonDiagParameter("Static IP");
    public static readonly CetonDiagParameter Netmask = new CetonDiagParameter("Netmask");
    public static readonly CetonDiagParameter DhcpClient = new CetonDiagParameter("DHCP Client");
	  public static readonly CetonDiagParameter WmDrmStatus = new CetonDiagParameter("WMDRM Status");
    public static readonly CetonDiagParameter WmDrmVersion = new CetonDiagParameter("WMDRM Version");

    private CetonDiagParameter(string name)
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
      CetonDiagParameter diagParam = obj as CetonDiagParameter;
      if (diagParam != null && this == diagParam)
      {
        return true;
      }
      return false;
    }

    public static ICollection<CetonDiagParameter> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator CetonDiagParameter(string name)
    {
      CetonDiagParameter value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(CetonDiagParameter diagParam)
    {
      return diagParam._name;
    }
  }

  public sealed class DriDiagParameter
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriDiagParameter> _values = new Dictionary<string, DriDiagParameter>();

    /// <summary>
    /// Serial Number of the DRIT.
    /// </summary>
    public static readonly DriDiagParameter HostSerialNumber = new DriDiagParameter("Host Serial Number");
    /// <summary>
    /// Unique ID of the DRIT used for Card/Host binding.
    /// </summary>
    public static readonly DriDiagParameter HostId = new DriDiagParameter("Host ID");
    /// <summary>
    /// Explicit description of the current power status.
    /// </summary>
    public static readonly DriDiagParameter HostPowerStatus = new DriDiagParameter("Host Power Status");
    /// <summary>
    /// Explicit description of the current boot status.
    /// </summary>
    public static readonly DriDiagParameter HostBootStatus = new DriDiagParameter("Host Boot Status");
    /// <summary>
    /// Explicit description of the current memory allocation.
    /// </summary>
    public static readonly DriDiagParameter HostMemoryReport = new DriDiagParameter("Host Memory Report");
    /// <summary>
    /// Explicit description of the DRM application supported by the devices, including name, version number and date.
    /// </summary>
    public static readonly DriDiagParameter HostApplication = new DriDiagParameter("Host Application");
    /// <summary>
    /// Explicit description of the DRIT firmware including name, version number and date.
    /// </summary>
    public static readonly DriDiagParameter HostFirmware = new DriDiagParameter("Host Firmware");

    private DriDiagParameter(string name)
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
      DriDiagParameter diagParam = obj as DriDiagParameter;
      if (diagParam != null && this == diagParam)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriDiagParameter> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriDiagParameter(string name)
    {
      DriDiagParameter value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriDiagParameter diagParam)
    {
      return diagParam._name;
    }
  }

  public class DiagService : BaseService
  {
    private CpAction _getParameterAction = null;

    public DiagService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:Diag")
    {
      _service.Actions.TryGetValue("GetParameter", out _getParameterAction);
    }

    /// <summary>
    /// Upon receipt of the GetParameter action, the DRIT SHALL return the value and the type of the parameter in less
    /// than 1s.
    /// </summary>
    /// <param name="parameter">This argument sets the A_ARG_TYPE_Parameter state variable.</param>
    /// <param name="value">This argument provides the value of the A_ARG_TYPE_Value state variable when the action response is created.</param>
    /// <param name="isVolatile">This argument provides the value of the A_ARG_TYPE_Volatile state variable when the action response is created.</param>
    public void GetParameter(DriDiagParameter parameter, out string value, out bool isVolatile)
    {
      IList<object> outParams = _getParameterAction.InvokeAction(new List<object> { parameter.ToString() });
      value = (string)outParams[0];
      isVolatile = (bool)outParams[1];
    }

    /// <summary>
    /// Upon receipt of the GetParameter action, the DRIT SHALL return the value and the type of the parameter in less
    /// than 1s.
    /// </summary>
    /// <param name="parameter">This argument sets the A_ARG_TYPE_Parameter state variable.</param>
    /// <param name="value">This argument provides the value of the A_ARG_TYPE_Value state variable when the action response is created.</param>
    /// <param name="isVolatile">This argument provides the value of the A_ARG_TYPE_Volatile state variable when the action response is created.</param>
    public void GetParameter(CetonDiagParameter parameter, out string value, out bool isVolatile)
    {
      IList<object> outParams = _getParameterAction.InvokeAction(new List<object> { parameter.ToString() });
      value = (string)outParams[0];
      isVolatile = (bool)outParams[1];
    }
  }
}
