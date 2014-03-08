#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Xml;
using System.Globalization;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;

namespace Mediaportal.TV.Server.TVLibrary.SatIp.Server
{
  public class LightServerDevice : DvDevice
  {
    public const string LIGHT_SERVER_DEVICE_TYPE = "ses-com:device:SatIPServer";
    public const int LIGHT_SERVER_DEVICE_TYPE_VERSION = 1;

    public LightServerDevice(string serverId) : base(LIGHT_SERVER_DEVICE_TYPE, LIGHT_SERVER_DEVICE_TYPE_VERSION, serverId, new LightServerDeviceInformation())
    {
      DescriptionGenerateHook += GenerateDescriptionFunc;
    }

    private static void GenerateDescriptionFunc(XmlWriter writer, DvDevice device, GenerationPosition pos,
                                                EndpointConfiguration config, CultureInfo culture)
    {
      if (pos == GenerationPosition.AfterDeviceList)
      {
        // TODO: make the "DVBT-2,DVBT2-2" dynamic
        writer.WriteElementString("satip", "X_SATIPCAP", "urn:ses-com:satip", "DVBS2-2");
      }
    }
  }
}