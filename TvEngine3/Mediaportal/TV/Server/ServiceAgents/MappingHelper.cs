using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public static class MappingHelper
  {
    public static GroupMap AddChannelToGroup(Channel channel, ChannelGroup @group, MediaTypeEnum mediaType)
    {
      foreach (GroupMap groupMap in channel.GroupMaps.Where(groupMap => groupMap.idGroup == @group.idGroup))
      {
        //already associated ?
        return groupMap;
      }
      Channel channelDetached = ChannelFactory.Clone(channel);
      DoAddChannelToGroup(channelDetached, @group, mediaType);      
      channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channelDetached);      
      channel.AcceptChanges();
      return channel.GroupMaps.Where(gMap => gMap.idGroup == @group.idGroup).FirstOrDefault();
    }

    private static void DoAddChannelToGroup(Channel channel, ChannelGroup @group, MediaTypeEnum mediaType)
    {
      var groupMap = new GroupMap
                       {
                         //Channel = channel, // causes : AcceptChanges cannot continue because the object's key values conflict with another object in the ObjectStateManager. Make sure that the key values are unique before calling AcceptChanges.
                         //ChannelGroup = group,
                         idChannel = channel.idChannel,
                         idGroup = @group.idGroup,
                         mediaType = (int) mediaType,
                         SortOrder = channel.sortOrder
                       };
      channel.GroupMaps.Add(groupMap);
    }

    public static GroupMap AddChannelToGroup(Channel channel, string groupName, MediaTypeEnum mediaType)
    {
      ChannelGroup channelGroup = ServiceAgents.Instance.ChannelGroupServiceAgent.GetChannelGroupByNameAndMediaType(groupName, mediaType);
      if (channelGroup != null)
      {
        return AddChannelToGroup(channel, channelGroup, mediaType);
      }
      return null;
    }

    public static void AddChannelsToGroup(IEnumerable<Channel> channels, ChannelGroup group)
    {
      foreach (Channel channel in channels)
      {
        DoAddChannelToGroup(channel, group, (MediaTypeEnum) channel.mediaType);
      }
      ServiceAgents.Instance.ChannelServiceAgent.SaveChannels(channels);
    }

    public static ChannelMap AddChannelToCard(Channel channel, Card card, bool epg)
    {      
      foreach (ChannelMap channelMap in channel.ChannelMaps.Where(chMap => chMap.idCard == card.idCard))
      {
        //already associated ?
        return channelMap;
      }

      var map = new ChannelMap()
      {
        idChannel = channel.idChannel,
        idCard =  card.idCard,
        epgOnly = epg
      };
      Channel channelDetached = ChannelFactory.Clone(channel);

      channelDetached.ChannelMaps.Add(map);
      channel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(channelDetached);
      channel.AcceptChanges();
      return channel.ChannelMaps.Where(chMap => chMap.idCard == card.idCard).FirstOrDefault();
    }
  
  }
}
