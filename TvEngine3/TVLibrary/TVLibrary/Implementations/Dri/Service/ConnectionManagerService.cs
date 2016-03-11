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
using System.Linq;
using UPnP.Infrastructure.CP.Description;
using UPnP.Infrastructure.CP.DeviceTree;
using UPnP.Infrastructure.CP.SSDP;

namespace TvLibrary.Implementations.Dri.Service
{
  public enum UpnpConnectionStatus
  {
    OK,
    ContentFormatMismatch,
    InsufficientBandwidth,
    UnreliableChannel,
    Unknown
  }

  public enum UpnpConnectionDirection
  {
    Output,
    Input
  }

  public class ConnectionManagerService : BaseService
  {
    private CpAction _getProtocolInfoAction = null;
    private CpAction _prepareForConnectionAction = null;
    private CpAction _connectionCompleteAction = null;
    private CpAction _getCurrentConnectionIdsAction = null;
    private CpAction _getCurrentConnectionInfoAction = null;

    public ConnectionManagerService(CpDevice device)
      : base(device, "urn:upnp-org:serviceId:urn:schemas-upnp-org:service:ConnectionManager")
    {
      _service.Actions.TryGetValue("GetProtocolInfo", out _getProtocolInfoAction);
      _service.Actions.TryGetValue("PrepareForConnection", out _prepareForConnectionAction);
      _service.Actions.TryGetValue("ConnectionComplete", out _connectionCompleteAction);
      _service.Actions.TryGetValue("GetCurrentConnectionIDs", out _getCurrentConnectionIdsAction);
      _service.Actions.TryGetValue("GetCurrentConnectionInfo", out _getCurrentConnectionInfoAction);
    }

    public void GetProtocolInfo(out string source, out string sink)
    {
      IList<object> outParams = _getProtocolInfoAction.InvokeAction(null);
      // Note: both source and sink contain CSV data.
      source = (string)outParams[0];
      sink = (string)outParams[1];
    }

    public bool PrepareForConnection(string remoteProtocolInfo, string peerConnectionManager, Int32 peerConnectionId,
                                      UpnpConnectionDirection direction, out Int32 connectionId, out Int32 avTransportId,
                                      out Int32 rcsId)
    {
      connectionId = 0;
      avTransportId = 0;
      rcsId = 0;
      if (_prepareForConnectionAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a ConnectionManager PrepareForConnection action", _device.UDN);
        return false;
      }

      IList<object> outParams = _prepareForConnectionAction.InvokeAction(new List<object> {
        remoteProtocolInfo, peerConnectionManager, peerConnectionId, direction.ToString()
      });
      connectionId = (int)outParams[0];
      avTransportId = (int)outParams[1];
      rcsId = (int)outParams[2];
      return true;
    }

    public bool ConnectionComplete(Int32 connectionId)
    {
      if (_connectionCompleteAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a ConnectionManager ConnectionComplete action", _device.UDN);
        return false;
      }
      _connectionCompleteAction.InvokeAction(new List<object> { connectionId });
      return true;
    }

    public void GetCurrentConnectionIds(out IList<UInt32> currentConnectionIds)
    {
      IList<object> outParams = _getCurrentConnectionIdsAction.InvokeAction(null);
      currentConnectionIds = outParams[0].ToString().Split(',').Select(x => UInt32.Parse(x)).ToList<UInt32>();
    }

    public void GetCurrentConnectionInfo(Int32 connectionId, out Int32 rcsId, out Int32 avTransportId,
                                        out string protocolInfo, out string peerConnectionManager,
                                        out Int32 peerConnectionId, out UpnpConnectionDirection direction,
                                        out UpnpConnectionStatus status)
    {
      IList<object> outParams = _getCurrentConnectionInfoAction.InvokeAction(new List<object> { connectionId });
      rcsId = (int)outParams[0];
      avTransportId = (int)outParams[1];
      protocolInfo = (string)outParams[2];
      peerConnectionManager = (string)outParams[3];
      peerConnectionId = (int)outParams[4];
      direction = (UpnpConnectionDirection)Enum.Parse(typeof(UpnpConnectionDirection), (string)outParams[5]);
      status = (UpnpConnectionStatus)Enum.Parse(typeof(UpnpConnectionStatus), (string)outParams[6]);
    }
  }
}
