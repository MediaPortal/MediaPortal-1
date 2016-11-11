using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{  
  public class ChannelGroupService : IChannelGroupService
  {    
    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupRelation includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupRelation includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaType, includeRelations);
    }

    public ChannelGroup GetChannelGroup(int idChannelGroup, ChannelGroupRelation includeRelations)
    {
      return ChannelGroupManagement.GetChannelGroup(idChannelGroup, includeRelations);
    }

    public ChannelGroup GetOrCreateChannelGroup(string name, MediaType mediaType)
    {
      return ChannelGroupManagement.GetOrCreateChannelGroup(name, mediaType);
    }

    public ChannelGroup SaveChannelGroup(ChannelGroup channelGroup)
    {
      return ChannelGroupManagement.SaveChannelGroup(channelGroup);
    }

    public void DeleteChannelGroup(int idChannelGroup)
    {
      ChannelGroupManagement.DeleteChannelGroup(idChannelGroup);
    }
  }
}