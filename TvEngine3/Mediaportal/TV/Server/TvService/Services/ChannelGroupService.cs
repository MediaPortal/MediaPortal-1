using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Services
{  
  public class ChannelGroupService : IChannelGroupService
  {    
    public IList<ChannelGroup> ListAllChannelGroups()
    {
      var listAllChannelGroups = ChannelGroupManagement.ListAllChannelGroups();
      return listAllChannelGroups;
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      var listAllChannelGroupsByMediaType = ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaType);
      return listAllChannelGroupsByMediaType;
    }

    public ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      var channelGroupByNameAndMediaType = ChannelGroupManagement.GetChannelGroupByNameAndMediaType(groupName, mediaType);
      return channelGroupByNameAndMediaType;
    }
       
    public void DeleteChannelGroupMap(int idMap)
    {
      ChannelGroupManagement.DeleteChannelGroupMap(idMap);
    }

    public ChannelGroup GetChannelGroup(int id)
    {
      return ChannelGroupManagement.GetChannelGroup(id);
    }

    public ChannelGroup SaveGroup(ChannelGroup group)
    {
      return ChannelGroupManagement.SaveGroup(group);
    }

    public void DeleteChannelGroup(int idGroup)
    {
      ChannelGroupManagement.DeleteChannelGroup(idGroup);
    }

    public ChannelGroup GetOrCreateGroup(string groupName)
    {
      var orCreateGroup = ChannelGroupManagement.GetOrCreateGroup(groupName);
      return orCreateGroup;
    }
  }
}
