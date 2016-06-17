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

    public IList<ChannelGroup> ListAllChannelGroups(ChannelGroupRelation includeRelations)
    {
      return _channel.ListAllChannelGroups(includeRelations);
    }

    public IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupRelation includeRelations)
    {
      return _channel.ListAllChannelGroupsByMediaType(mediaType, includeRelations);
    }

    public ChannelGroup GetChannelGroup(int idChannelGroup, ChannelGroupRelation includeRelations)
    {
      return _channel.GetChannelGroup(idChannelGroup, includeRelations);
    }

    public ChannelGroup GetOrCreateChannelGroup(string name, MediaType mediaType)
    {
      return _channel.GetOrCreateChannelGroup(name, mediaType);
    }

    public ChannelGroup SaveChannelGroup(ChannelGroup channelGroup)
    {
      channelGroup.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelGroup(channelGroup);
    }

    public void DeleteChannelGroup(int idChannelGroup)
    {
      _channel.DeleteChannelGroup(idChannelGroup);
    }
  }
}