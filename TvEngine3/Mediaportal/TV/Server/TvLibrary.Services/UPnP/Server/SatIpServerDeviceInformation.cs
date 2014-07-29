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
using System.Globalization;
using System.Net;
using UPnP.Infrastructure.Dv.DeviceTree;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.ResourceAccess;

namespace Mediaportal.TV.Server.TVLibrary.TVEUPnPServer.Server
{
  public class SatIpServerDeviceInformation : ILocalizedDeviceInformation
  {
    public string GetFriendlyName(CultureInfo culture)
    {
      return "MediaPortal SAT>IP Server";
    }

    public string GetManufacturer(CultureInfo culture)
    {
      return "Team MediaPortal";
    }

    public string GetManufacturerURL(CultureInfo culture)
    {
      return "http://www.team-mediaportal.com";
    }

    public string GetModelDescription(CultureInfo culture)
    {
      return null;
    }

    public string GetModelName(CultureInfo culture)
    {
      return "MediaPortal SAT>IP Server";
    }

    public string GetModelNumber(CultureInfo culture)
    {
      return "1";
    }

    public string GetModelURL(CultureInfo culture)
    {
      return null;
    }

    public string GetSerialNumber(CultureInfo culture)
    {
      return null;
    }

    public string GetUPC()
    {
      return null;
    }

    public ICollection<IconDescriptor> GetIcons(CultureInfo culture)
    {
      List<IconDescriptor> icons = new List<IconDescriptor>();
      IconDescriptor icon1 = new IconDescriptor
      {
        MimeType = "image/png",
        Height = 120,
        Width = 120,
        ColorDepth = 32,
        GetIconURLDelegate = BuildIconUrl120Png
      };
      IconDescriptor icon2 = new IconDescriptor
      {
        MimeType = "image/png",
        Height = 48,
        Width = 48,
        ColorDepth = 32,
        GetIconURLDelegate = BuildIconUrl48Png
      };
      IconDescriptor icon3 = new IconDescriptor
      {
        MimeType = "image/jpeg",
        Width = 120,
        Height = 120,
        ColorDepth = 24,
        GetIconURLDelegate = BuildIconUrl120Jpeg
      };
      IconDescriptor icon4 = new IconDescriptor
      {
        MimeType = "image/jpeg",
        Height = 48,
        Width = 48,
        ColorDepth = 24,
        GetIconURLDelegate = BuildIconUrl48Jpeg
      };
      icons.Add(icon1);
      icons.Add(icon2);
      icons.Add(icon3);
      icons.Add(icon4);
      return icons;
    }

    #region IconDelegates

    private string BuildIconUrl120Png(IPAddress endpointipaddress, CultureInfo culture)
    {
      return string.Format("{0}/icons/icon_120.png",
        UPnPResourceAccessUtils.GetStaticBaseResourceUrlFromEndpoint(/*endpointipaddress*/));
    }
    private string BuildIconUrl48Png(IPAddress endpointipaddress, CultureInfo culture)
    {
      return string.Format("{0}/icons/icon_48.png",
        UPnPResourceAccessUtils.GetStaticBaseResourceUrlFromEndpoint(/*endpointipaddress*/));
    }
    private string BuildIconUrl120Jpeg(IPAddress endpointipaddress, CultureInfo culture)
    {
      return string.Format("{0}/icons/icon_120.jpg",
        UPnPResourceAccessUtils.GetStaticBaseResourceUrlFromEndpoint(/*endpointipaddress*/));
    }
    private string BuildIconUrl48Jpeg(IPAddress endpointipaddress, CultureInfo culture)
    {
      return string.Format("{0}/icons/icon_48.jpg",
        UPnPResourceAccessUtils.GetStaticBaseResourceUrlFromEndpoint(/*endpointipaddress*/));
    }

    #endregion IconDelegates
  }
}