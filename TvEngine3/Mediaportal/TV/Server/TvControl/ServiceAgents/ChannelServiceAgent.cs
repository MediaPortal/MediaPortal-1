using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.ServiceAgents
{
  public class ChannelServiceAgent : ServiceAgent<IChannelService>, IChannelService
  {
    public ChannelServiceAgent(string hostname) : base(hostname)
    {
    }

    public IList<Channel> ListAllChannels(ChannelRelation includeRelations)
    {
      return _channel.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      return _channel.ListAllChannelsByGroupId(idChannelGroup, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      return _channel.ListAllVisibleChannelsByGroupId(idChannelGroup, includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      return _channel.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      return _channel.ListAllVisibleChannelsByMediaType(mediaType, includeRelations);
    }

    public Channel GetChannel(int idChannel, ChannelRelation includeRelations)
    {
      return _channel.GetChannel(idChannel, includeRelations);
    }

    public IList<Channel> GetChannelsByName(string name, ChannelRelation includeRelations)
    {
      return _channel.GetChannelsByName(name, includeRelations);
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

    public Channel MergeChannels(IEnumerable<Channel> channels, ChannelRelation includeRelations)
    {
      return _channel.MergeChannels(channels, includeRelations);
    }

    #region tuning details

    public IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations)
    {
      return _channel.ListAllTuningDetailsByChannel(idChannel, includeRelations);
    }

    public IList<TuningDetail> ListAllDigitalTransmitterTuningDetails()
    {
      return _channel.ListAllDigitalTransmitterTuningDetails();
    }

    public TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations)
    {
      return _channel.GetTuningDetail(idTuningDetail, includeRelations);
    }

    public IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations)
    {
      return _channel.GetAnalogTelevisionTuningDetails(physicalChannelNumber, includeRelations);
    }

    public IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null)
    {
      return _channel.GetAtscScteTuningDetails(broadcastStandard, logicalChannelNumber, includeRelations, frequency);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations)
    {
      return _channel.GetCaptureTuningDetails(name, includeRelations);
    }

    public IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetDvbTuningDetails(broadcastStandard, originalNetworkId, serviceId, includeRelations, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      return _channel.GetFmRadioTuningDetails(frequency, includeRelations);
    }

    public IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      return _channel.GetFreesatTuningDetails(channelId, includeRelations);
    }

    public IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetMpeg2TuningDetails(broadcastStandard, programNumber, includeRelations, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetOpenTvTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      return _channel.GetOpenTvTuningDetails(channelId, includeRelations);
    }

    public IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations)
    {
      return _channel.GetStreamTuningDetails(url, includeRelations);
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