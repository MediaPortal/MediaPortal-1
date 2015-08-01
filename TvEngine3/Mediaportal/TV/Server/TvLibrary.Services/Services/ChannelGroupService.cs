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
    public IList<ChannelGroup> ListAllChannelGroups()
    {
      return ChannelGroupManagement.ListAllChannelGroups();
    }

    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType)
    {
      return ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaType);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaTypeEnum, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return ChannelGroupManagement.ListAllChannelGroupsByMediaType(mediaTypeEnum, includeRelations);
    }

    public ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaType mediaType)
    {
      return ChannelGroupManagement.GetChannelGroupByNameAndMediaType(groupName, mediaType);
    }

    public ChannelGroup GetChannelGroup(int id)
    {
      return ChannelGroupManagement.GetChannelGroup(id);
    }

    public ChannelGroup GetChannelGroup(int id, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return ChannelGroupManagement.GetChannelGroup(id, includeRelations);
    }

    public ChannelGroup SaveGroup(ChannelGroup group)
    {
      return ChannelGroupManagement.SaveGroup(group);
    }

    public void DeleteChannelGroup(int idGroup)
    {
      ChannelGroupManagement.DeleteChannelGroup(idGroup);
    }

    public ChannelGroup GetOrCreateGroup(string groupName, MediaType mediaType)
    {
      return ChannelGroupManagement.GetOrCreateGroup(groupName, mediaType);
    }
  }
}