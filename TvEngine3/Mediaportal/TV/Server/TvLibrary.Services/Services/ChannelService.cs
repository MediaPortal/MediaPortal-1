using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ChannelService : IChannelService
  {
    public IList<Channel> ListAllChannels()
    {
      return ChannelManagement.ListAllChannels();
    }

    public IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId)
    {
      return ChannelManagement.ListAllChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId)
    {
      return ChannelManagement.ListAllVisibleChannelsByGroupId(groupId);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByGroupId(groupId, includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType)
    {
      return ChannelManagement.ListAllChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType)
    {
      return ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType, includeRelations);
    }

    public Channel GetChannel(int idChannel)
    {
      return ChannelManagement.GetChannel(idChannel);
    }

    public Channel GetChannel(int idChannel, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.GetChannel(idChannel, includeRelations);
    }

    public IList<Channel> GetChannelsByName(string channelName)
    {
      return ChannelManagement.GetChannelsByName(channelName);
    }

    public IList<Channel> GetChannelsByName(string channelName, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.GetChannelsByName(channelName, includeRelations);
    }

    public Channel SaveChannel(Channel channel)
    {
      return ChannelManagement.SaveChannel(channel);
    }

    public IList<Channel> SaveChannels(IEnumerable<Channel> channels)
    {
      return ChannelManagement.SaveChannels(channels);
    }

    public void DeleteChannel(int idChannel)
    {
      ChannelManagement.DeleteChannel(idChannel);
    }

    public Channel MergeChannels(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations)
    {
      return ChannelManagement.MergeChannels(channels, includeRelations);
    }

    #region tuning details

    public IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel)
    {
      return TuningDetailManagement.ListAllTuningDetailsByChannel(idChannel);
    }

    public TuningDetail GetTuningDetail(int idTuningDetail)
    {
      return TuningDetailManagement.GetTuningDetail(idTuningDetail);
    }

    public IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber)
    {
      return TuningDetailManagement.GetAnalogTelevisionTuningDetails(physicalChannelNumber);
    }

    public IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, int? frequency = null)
    {
      return TuningDetailManagement.GetAtscScteTuningDetails(broadcastStandard, logicalChannelNumber, frequency);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(string name)
    {
      return TuningDetailManagement.GetCaptureTuningDetails(name);
    }

    public IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return TuningDetailManagement.GetDvbTuningDetails(broadcastStandard, originalNetworkId, serviceId, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetFmRadioTuningDetails(int frequency)
    {
      return TuningDetailManagement.GetFmRadioTuningDetails(frequency);
    }

    public IList<TuningDetail> GetFreesatTuningDetails(int channelId)
    {
      return TuningDetailManagement.GetFreesatTuningDetails(channelId);
    }

    public IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return TuningDetailManagement.GetMpeg2TuningDetails(broadcastStandard, programNumber, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetOpenTvTuningDetails(int channelId)
    {
      return TuningDetailManagement.GetOpenTvTuningDetails(channelId);
    }

    public IList<TuningDetail> GetStreamTuningDetails(string url)
    {
      return TuningDetailManagement.GetStreamTuningDetails(url);
    }

    public void AddTuningDetail(int idChannel, IChannel channel)
    {
      TuningDetailManagement.AddTuningDetail(idChannel, channel);
    }

    public void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel)
    {
      TuningDetailManagement.UpdateTuningDetail(idChannel, idTuning, channel);
    }

    public TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      return TuningDetailManagement.SaveTuningDetail(tuningDetail);
    }

    public void DeleteTuningDetail(int idTuning)
    {
      TuningDetailManagement.DeleteTuningDetail(idTuning);
    }

    #endregion

    #region channel-to-tuner maps

    public ChannelMap SaveChannelMap(ChannelMap channelMap)
    {
      return ChannelManagement.SaveChannelMap(channelMap);
    }

    public IList<ChannelMap> SaveChannelMaps(IEnumerable<ChannelMap> channelMaps)
    {
      return ChannelManagement.SaveChannelMaps(channelMaps);
    }

    public void DeleteChannelMap(int idChannelMap)
    {
      ChannelManagement.DeleteChannelMap(idChannelMap);
    }

    public void DeleteChannelMaps(IEnumerable<int> channelMapIds)
    {
      ChannelManagement.DeleteChannelMaps(channelMapIds);
    }

    #endregion

    #region channel-to-group maps

    public GroupMap SaveChannelGroupMap(GroupMap groupMap)
    {
      return ChannelManagement.SaveChannelGroupMap(groupMap);
    }

    public IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps)
    {
      return ChannelManagement.SaveChannelGroupMaps(groupMaps);
    }

    public void DeleteChannelGroupMap(int idGroupMap)
    {
      ChannelManagement.DeleteChannelGroupMap(idGroupMap);
    }

    public void DeleteChannelGroupMaps(IEnumerable<int> groupMapIds)
    {
      ChannelManagement.DeleteChannelGroupMaps(groupMapIds);
    }

    #endregion
  }
}