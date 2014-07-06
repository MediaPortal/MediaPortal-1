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

using UPnP.Infrastructure.Dv;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Server
{
  /// <summary>
  /// Encapsulates the UPnP light device.
  /// </summary>
  public class UPnPLightServer : UPnPServer
  {
    public const int SSDP_ADVERTISMENT_INTERVAL = 180;

    public UPnPLightServer()
    {
      // SAT>IP root device
      AddRootDevice(new LightServerDevice(SettingsManagement.GetValue("SATIP_UDN", System.Guid.NewGuid().ToString("D"))));
      // UPnP root device
      AddRootDevice(new UPnPMediaServerDevice(SettingsManagement.GetValue("UPNP_UDN", System.Guid.NewGuid().ToString("D"))));
    }

    public void Start()
    {
      Bind(SSDP_ADVERTISMENT_INTERVAL);
    }

    public void Stop()
    {
      Close();
    }
  }
}