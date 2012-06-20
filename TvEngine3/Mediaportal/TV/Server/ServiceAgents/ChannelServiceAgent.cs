using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVService.ServiceAgents
{
  public class ChannelServiceAgent : ServiceAgent<IChannelService>, IChannelService
  {
    public ChannelServiceAgent(string hostname) : base(hostname)
    {      
    }

    public IList<Channel> GetAllChannelsByGroupIdAndMediaType(int groupId, MediaTypeEnum mediatype)
    {
      return _channel.GetAllChannelsByGroupIdAndMediaType(groupId, mediatype);
    }

    public IList<Channel> GetAllChannelsByGroupId(int groupId)
    {
      return _channel.GetAllChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllChannels()
    {
      return _channel.ListAllChannels();
    }

    public IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      foreach (var channel in channels)
      {
        channel.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveChannels(channels);
    }

    public IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      /*foreach (var groupMap in groupMaps)
      {
        groupMap.UnloadAllUnchangedRelationsForEntity();
      }*/
      return _channel.SaveChannelGroupMaps(groupMaps);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType)
    {
      return _channel.ListAllChannelsByMediaType(mediaType);
    }

    public IList<Channel> GetChannelsByName(string channelName)
    {
      return _channel.GetChannelsByName(channelName);
    }

    public Channel SaveChannel(Channel channel)
    {
      channel.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannel(channel);
    }

    public Channel GetChannel(int idChannel)
    {
      return _channel.GetChannel(idChannel);
    }

    public void DeleteChannel(int idChannel)
    {
      _channel.DeleteChannel(idChannel);
    }

    public TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      tuningDetail.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTuningDetail(tuningDetail);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType)
    {
      return _channel.ListAllVisibleChannelsByMediaType(mediaType);
    }

    public TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel)
    {
      return _channel.GetTuningDetail(dvbChannel);
    }

    public TuningDetail GetTuningDetailCustom(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum)
    {
      return _channel.GetTuningDetailCustom(dvbChannel, tuningDetailSearchEnum);
    }

    public TuningDetail GetTuningDetailByURL(DVBBaseChannel dvbChannel, string url)
    {
      return _channel.GetTuningDetailByURL(dvbChannel, url);
    }

    public void AddTuningDetail(int idChannel, IChannel channel)
    {
      _channel.AddTuningDetail(idChannel, channel);
    }

    public IList<TuningDetail> GetTuningDetailsByName(string channelName, int channelType)
    {
      return _channel.GetTuningDetailsByName(channelName, channelType);
    }

    public void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel)
    {
      _channel.UpdateTuningDetail(idChannel, idTuning, channel);
    }

    public Channel GetChannelByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.GetChannelByName(channelName, includeRelations);
    }

    public void DeleteTuningDetail(int idTuning)
    {
      _channel.DeleteTuningDetail(idTuning);
    }

    public GroupMap SaveChannelGroupMap(GroupMap groupMap)
    {
      groupMap.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelGroupMap(groupMap);
    }

    public void DeleteChannelMap(int idChannelMap)
    {
      _channel.DeleteChannelMap(idChannelMap);
    }

    public ChannelMap SaveChannelMap(ChannelMap map)
    {
      map.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelMap(map);
    }

    public IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaTypeEnum, ChannelIncludeRelationEnum include)
    {
      return _channel.GetAllChannelsByGroupIdAndMediaType(idGroup, mediaTypeEnum, include);
    }
  }
}
