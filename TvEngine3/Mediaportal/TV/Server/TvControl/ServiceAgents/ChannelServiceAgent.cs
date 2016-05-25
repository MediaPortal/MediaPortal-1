using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ChannelServiceAgent : ServiceAgent<IChannelService>, IChannelService
  {
    public ChannelServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Channel> ListAllChannels()
    {
      return _channel.ListAllChannels();
    }

    public IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId)
    {
      return _channel.ListAllChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId)
    {
      return _channel.ListAllVisibleChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllVisibleChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType)
    {
      return _channel.ListAllChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType)
    {
      return _channel.ListAllVisibleChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.ListAllVisibleChannelsByMediaType(mediaType, includeRelations);
    }

    public Channel GetChannel(int idChannel)
    {
      return _channel.GetChannel(idChannel);
    }

    public Channel GetChannel(int idChannel, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.GetChannel(idChannel, includeRelations);
    }

    public IList<Channel> GetChannelsByName(string channelName)
    {
      return _channel.GetChannelsByName(channelName);
    }

    public IList<Channel> GetChannelsByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.GetChannelsByName(channelName, includeRelations);
    }

    public Channel SaveChannel(Channel channel)
    {
      channel.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannel(channel);
    }

    public IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      foreach (var channel in channels)
      {
        channel.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveChannels(channels);
    }

    public void DeleteChannel(int idChannel)
    {
      _channel.DeleteChannel(idChannel);
    }

    public Channel MergeChannels(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations)
    {
      return _channel.MergeChannels(channels, includeRelations);
    }

    #region tuning details

    public IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel)
    {
      return _channel.ListAllTuningDetailsByChannel(idChannel);
    }

    public TuningDetail GetTuningDetail(int idTuningDetail)
    {
      return _channel.GetTuningDetail(idTuningDetail);
    }

    public IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber)
    {
      return _channel.GetAnalogTelevisionTuningDetails(physicalChannelNumber);
    }

    public IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, int? frequency = null)
    {
      return _channel.GetAtscScteTuningDetails(broadcastStandard, logicalChannelNumber, frequency);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(string name)
    {
      return _channel.GetCaptureTuningDetails(name);
    }

    public IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetDvbTuningDetails(broadcastStandard, originalNetworkId, serviceId, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetFmRadioTuningDetails(int frequency)
    {
      return _channel.GetFmRadioTuningDetails(frequency);
    }

    public IList<TuningDetail> GetFreesatTuningDetails(int channelId)
    {
      return _channel.GetFreesatTuningDetails(channelId);
    }

    public IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetMpeg2TuningDetails(broadcastStandard, programNumber, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetOpenTvTuningDetails(int channelId)
    {
      return _channel.GetOpenTvTuningDetails(channelId);
    }

    public IList<TuningDetail> GetStreamTuningDetails(string url)
    {
      return _channel.GetStreamTuningDetails(url);
    }

    public void AddTuningDetail(int idChannel, IChannel channel)
    {
      _channel.AddTuningDetail(idChannel, channel);
    }

    public void UpdateTuningDetail(int idChannel, int idTuningDetail, IChannel channel)
    {
      _channel.UpdateTuningDetail(idChannel, idTuningDetail, channel);
    }

    public TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      tuningDetail.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveTuningDetail(tuningDetail);
    }

    public void DeleteTuningDetail(int idTuningDetail)
    {
      _channel.DeleteTuningDetail(idTuningDetail);
    }

    #endregion

    #region channel-to-tuner maps

    public ChannelMap SaveChannelMap(ChannelMap channelMap)
    {
      channelMap.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelMap(channelMap);
    }

    public IList<ChannelMap> SaveChannelMaps(IEnumerable<ChannelMap> channelMaps)
    {
      foreach (ChannelMap channelMap in channelMaps)
      {
        channelMap.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveChannelMaps(channelMaps);
    }

    public void DeleteChannelMap(int idChannelMap)
    {
      _channel.DeleteChannelMap(idChannelMap);
    }

    public void DeleteChannelMaps(IEnumerable<int> channelMapIds)
    {
      _channel.DeleteChannelMaps(channelMapIds);
    }

    #endregion

    #region channel-to-group maps

    public GroupMap SaveChannelGroupMap(GroupMap groupMap)
    {
      groupMap.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelGroupMap(groupMap);
    }

    public IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      foreach (GroupMap groupMap in groupMaps)
      {
        groupMap.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveChannelGroupMaps(groupMaps);
    }

    public void DeleteChannelGroupMap(int idGroupMap)
    {
      _channel.DeleteChannelGroupMap(idGroupMap);
    }

    public void DeleteChannelGroupMaps(IEnumerable<int> groupMapIds)
    {
      _channel.DeleteChannelGroupMaps(groupMapIds);
    }

    #endregion
  }
}