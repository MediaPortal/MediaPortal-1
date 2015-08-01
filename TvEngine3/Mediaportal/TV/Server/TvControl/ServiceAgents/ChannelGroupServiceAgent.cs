using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ChannelGroupServiceAgent : ServiceAgent<IChannelGroupService>, IChannelGroupService
  {
    public ChannelGroupServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<ChannelGroup> ListAllChannelGroups()
    {
      return _channel.ListAllChannelGroups();
    }

    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType)
    {
      return _channel.ListAllChannelGroupsByMediaType(mediaType);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelGroupsByMediaType(mediaType, includeRelations);
    }

    public ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaType mediaType)
    {
      return _channel.GetChannelGroupByNameAndMediaType(groupName, mediaType);
    }

    public ChannelGroup GetOrCreateGroup(string groupName, MediaType mediaType)
    {
      return _channel.GetOrCreateGroup(groupName, mediaType);
    }

    public ChannelGroup GetChannelGroup(int id)
    {
      return _channel.GetChannelGroup(id);
    }

    public ChannelGroup GetChannelGroup(int id, ChannelGroupIncludeRelationEnum includeRelations)
    {
      return _channel.GetChannelGroup(id, includeRelations);
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
  }
}