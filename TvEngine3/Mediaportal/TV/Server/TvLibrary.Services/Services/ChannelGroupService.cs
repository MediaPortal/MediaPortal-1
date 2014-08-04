using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{  
  public class ChannelGroupService : IChannelGroupService
  {    
    public IList<ChannelGroup> ListAllChannelGroups()
    {
      return ChannelGroupManagement.ListAllChannelGroups();
    }

    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      return ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaType);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaTypeEnum, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaTypeEnum, includeRelations);
    }

    public ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      return ChannelGroupManagement.GetChannelGroupByNameAndMediaType(groupName, mediaType);
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

    public ChannelGroup GetOrCreateGroup(string groupName, MediaTypeEnum mediaType)
    {
      return ChannelGroupManagement.GetOrCreateGroup(groupName, mediaType);
    }
  }
}