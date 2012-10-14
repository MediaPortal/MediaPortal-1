using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class ChannelGroupServiceAgent : ServiceAgent<IChannelGroupService>, IChannelGroupService
  {
    public ChannelGroupServiceAgent(string hostname) : base(hostname)
    {
    }
  
    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType)
    {
      return _channel.ListAllChannelGroupsByMediaType(mediaType);
    }

    public ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType)
    {
      return _channel.GetChannelGroupByNameAndMediaType(groupName, mediaType);
    }

    public ChannelGroup GetOrCreateGroup(string groupName, MediaTypeEnum mediaType)
    {
      return _channel.GetOrCreateGroup(groupName, mediaType);
    }

    public void DeleteChannelGroupMap(int idMap)
    {
      _channel.DeleteChannelGroupMap(idMap);
    }

    public ChannelGroup GetChannelGroup(int id)
    {
      return _channel.GetChannelGroup(id);
    }

    public ChannelGroup SaveGroup(ChannelGroup @group)
    {
      @group.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveGroup(@group);
    }

    public void DeleteChannelGroup(int idGroup)
    {
      _channel.DeleteChannelGroup(idGroup);
    }   

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelGroupsByMediaType(mediaType, includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroups()
    {
      return _channel.ListAllChannelGroups();
    }

    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllCustomChannelGroups(ChannelGroupIncludeRelationEnum includeRelations, MediaTypeEnum mediaType)
    {
      return _channel.ListAllCustomChannelGroups(includeRelations, mediaType);
    }
  }
}
