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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;
using HttpServer;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects.Basic;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects.MediaLibrary;
using MediaPortal.TV.Server.TVLibrary.UPnP.MediaServer.Objects;
//using MediaPortal.Extensions.MediaServer.ResourceAccess;
using Mediaportal.TV.Server.TVLibrary.UPnP.Server.Objects.Container;


namespace Mediaportal.TV.Server.TVLibrary.UPnP.Server
{
  public class UPnPMediaServerDevice : DvDevice
  {
    public const string MEDIASERVER_DEVICE_TYPE = "schemas-upnp-org:device:MediaServer";
    public const int MEDIASERVER_DEVICE_VERSION = 1;

    public const string CONTENT_DIRECTORY_SERVICE_TYPE = "schemas-upnp-org:service:ContentDirectory";
    public const int CONTENT_DIRECTORY_SERVICE_TYPE_VERSION = 1;
    public const string CONTENT_DIRECTORY_SERVICE_ID = "ContentDirectory";

    public const string CONNECTION_MANAGER_SERVICE_TYPE = "schemas-upnp-org:service:ConnectionManager";
    public const int CONNECTION_MANAGER_SERVICE_TYPE_VERSION = 1;
    public const string CONNECTION_MANAGER_SERVICE_ID = "ConnectionManager";

    private HttpServer.HttpServer Server;


    public UPnPMediaServerDevice(string deviceUuid)
      : base(MEDIASERVER_DEVICE_TYPE, MEDIASERVER_DEVICE_VERSION, deviceUuid,
             new UPnPMediaServerDeviceInformation())
    {
      DescriptionGenerateHook += GenerateDescriptionFunc;
      InitialiseContainerTree();
      AddService(new UPnPContentDirectoryServiceImpl());
      AddService(new UPnPConnectionManagerServiceImpl());
      /*Server = new HttpServer.HttpServer();
      Server.ServerName = "MP UPnP resource provider";
      Server.Add(new DlnaResourceAccessModule());
      Server.Start(IPAddress.Parse(MediaLibraryResource.GetLocalIp()), 456465);*/
    }

    private static void GenerateDescriptionFunc(XmlWriter writer, DvDevice device, GenerationPosition pos,
                                                EndpointConfiguration config, CultureInfo culture)
    {
      if (pos == GenerationPosition.AfterDeviceList)
      {
        writer.WriteElementString("dlna", "X_DLNADOC", "urn:schemas-dlna-org:device-1-0", "DMS-1.50");
      }
    }

    public static BasicContainer RootContainer { get; private set; }

    private static void InitialiseContainerTree()
    {
      RootContainer = new BasicContainer("0") { Title = "MediaPortal Media Library" };
      var channelContainer = new BasicContainer("V") { Title = "Channels" };
      RootContainer.Add(channelContainer);
      IList<ChannelGroup> groups = GlobalServiceProvider.Get<IChannelGroupService>().ListAllChannelGroupsByMediaType(MediaTypeEnum.TV, ChannelGroupIncludeRelationEnum.None);

      foreach (ChannelGroup group in groups)
      {
        var groupContainer = new BasicContainer("Group:" + group.IdGroup.ToString()) { Title = group.GroupName };
        IList<Channel> channels = GlobalServiceProvider.Get<IChannelService>().GetAllChannelsByGroupIdAndMediaType(group.IdGroup, MediaTypeEnum.TV);
        foreach (Channel channel in channels)
        {
          var resource = new MediaLibraryResource(channel.IdChannel);
          resource.Initialise();
          IList<IDirectoryResource> resources = new List<IDirectoryResource>();
          resources.Add(resource);
          groupContainer.Add(new BasicItem("Channel:" + channel.IdChannel.ToString()) { Title = channel.DisplayName, Resources = resources });
        }
        channelContainer.Add(groupContainer);
      }
    }
  }
}