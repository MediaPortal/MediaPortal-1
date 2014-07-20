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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MediaPortal.Common;
using UPnP.Infrastructure.Utils;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.DLNA;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects.MediaLibrary
{
  public class MediaLibraryResourceChannel : IDirectoryResource
  {
    private int Item { get; set; }

    public MediaLibraryResourceChannel(int item)
    {
      Item = item;
    }

    public static string GetLocalIp()
    {
      var localIp = Dns.GetHostName();
      var host = Dns.GetHostEntry(localIp);
      foreach (var ip in host.AddressList)
      {
        if (ip.AddressFamily.ToString() == "InterNetwork")
        {
          localIp = ip.ToString();
        }
      }
      return localIp;
    }

    public static string GetBaseResourceURL()
    {
      return "rtsp://" + GetLocalIp() + ":" + 554;
    }

    public void Initialise()
    {
      var url = GetBaseResourceURL() + getChannelUrl();

      ProtocolInfo = new DlnaProtocolInfo
      {
        Protocol = "rtsp-rtp-udp",
        Network = "*",
        MediaType = "video/mpeg",
        AdditionalInfo = new DlnaForthField()
      }.ToString(); 

      Uri = url;
    }

    private string getChannelUrl() {
      int pmtPid = GlobalServiceProvider.Get<IChannelService>().GetChannel(Item).TuningDetails[0].PmtPid;
      StringBuilder url = new StringBuilder();
      url.AppendFormat("/?channelId={0}", Item);
      url.AppendFormat("&pmtPid={0}", pmtPid);
      return url.ToString();
    }

    public string Uri { get; set; }

    public ulong Size { get; set; }

    public string Duration { get; set; }

    public uint BitRate { get; set; }

    public uint SampleFrequency { get; set; }

    public uint BitsPerSample { get; set; }

    public uint NumberOfAudioChannels { get; set; }

    public string Resolution { get; set; }

    public uint ColorDepth { get; set; }

    public string ProtocolInfo { get; set; }

    public string Protection { get; set; }

    public string ImportUri { get; set; }

    public string DlnaIfoFileUrl { get; set; }
  }
}