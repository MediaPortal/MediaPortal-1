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
using Mediaportal.TV.Server.TVLibrary.TVEUPnPServer;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.ResourceAccess;

namespace MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects.MediaLibrary
{
  class MediaLibraryResourceRecording : IDirectoryResource
  {
    private int Item { get; set; }

    public MediaLibraryResourceRecording(int item)
    {
      Item = item;
    }

    public static string GetBaseResourceURL()
    {
      return "http://" + UPnPResourceAccessUtils.LocalIPAddress() + ":" + MPUPnPServer.RESOURCE_SERVER_PORT + "/" + UPnPResourceAccessUtils.RESOURCE_RECORDING_ACCESS_PATH;
    }

    public void Initialise()
    {
      var url = GetBaseResourceURL() + getChannelUrl();

      ProtocolInfo = new DlnaProtocolInfo
      {
        Protocol = "http",
        Network = "*",
        MediaType = "video/mpeg",
        AdditionalInfo = new DlnaForthField()
      }.ToString(); 

      Uri = url;
    }

    private string getChannelUrl() {
      StringBuilder url = new StringBuilder();
      url.AppendFormat("/?recordingId={0}", Item);
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
