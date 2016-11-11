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

    public void DeleteOrphanedChannels(IEnumerable<int> channelIds = null)
    {
      _channel.DeleteOrphanedChannels(channelIds);
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

    public IList<TuningDetail> ListAllTuningDetailsByBroadcastStandard(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? idSatellite = null)
    {
      return _channel.ListAllTuningDetailsByBroadcastStandard(broadcastStandards, includeRelations, idSatellite);
    }

    public IList<TuningDetail> ListAllTuningDetailsByMediaType(MediaType mediaType, TuningDetailRelation includeRelations)
    {
      return _channel.ListAllTuningDetailsByMediaType(mediaType, includeRelations);
    }

    public IList<TuningDetail> ListAllTuningDetailsByOriginalNetworkIds(IEnumerable<int> originalNetworkIds, TuningDetailRelation includeRelations)
    {
      return _channel.ListAllTuningDetailsByOriginalNetworkIds(originalNetworkIds, includeRelations);
    }

    public IList<TuningDetail> ListAllDigitalTransmitterTuningDetails()
    {
      return _channel.ListAllDigitalTransmitterTuningDetails();
    }

    public TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations)
    {
      return _channel.GetTuningDetail(idTuningDetail, includeRelations);
    }

    public IList<TuningDetail> GetAmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      return _channel.GetAmRadioTuningDetails(frequency, includeRelations);
    }

    public IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations)
    {
      return _channel.GetAnalogTelevisionTuningDetails(physicalChannelNumber, includeRelations);
    }

    public IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandards, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null)
    {
      return _channel.GetAtscScteTuningDetails(broadcastStandards, logicalChannelNumber, includeRelations, frequency);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations)
    {
      return _channel.GetCaptureTuningDetails(name, includeRelations);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(int tunerId, TuningDetailRelation includeRelations)
    {
      return _channel.GetCaptureTuningDetails(tunerId, includeRelations);
    }

    public IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandards, int originalNetworkId, TuningDetailRelation includeRelations, int? serviceId = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetDvbTuningDetails(broadcastStandards, originalNetworkId, includeRelations, serviceId, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetExternalTunerTuningDetails(TuningDetailRelation includeRelations)
    {
      return _channel.GetExternalTunerTuningDetails(includeRelations);
    }

    public IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      return _channel.GetFmRadioTuningDetails(frequency, includeRelations);
    }

    public IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      return _channel.GetFreesatTuningDetails(channelId, includeRelations);
    }

    public IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? programNumber = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return _channel.GetMpeg2TuningDetails(broadcastStandards, includeRelations, programNumber, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetOpenTvTuningDetails(BroadcastStandard broadcastStandards, int channelId, TuningDetailRelation includeRelations)
    {
      return _channel.GetOpenTvTuningDetails(broadcastStandards, channelId, includeRelations);
    }

    public IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations)
    {
      return _channel.GetStreamTuningDetails(url, includeRelations);
    }

    public void UpdateTuningDetailEpgInfo(TuningDetail transmitterTuningDetail)
    {
      _channel.UpdateTuningDetailEpgInfo(transmitterTuningDetail);
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

    #region tuning-detail-to-tuner mappings

    public IList<TunerTuningDetailMapping> ListAllTunerMappings()
    {
      return _channel.ListAllTunerMappings();
    }

    public TunerTuningDetailMapping SaveTunerMapping(TunerTuningDetailMapping mapping)
    {
      return _channel.SaveTunerMapping(mapping);
    }

    public IList<TunerTuningDetailMapping> SaveTunerMappings(IEnumerable<TunerTuningDetailMapping> mappings)
    {
      return _channel.SaveTunerMappings(mappings);
    }

    public void DeleteTunerMapping(int idTunerMapping)
    {
      _channel.DeleteTunerMapping(idTunerMapping);
    }

    public void DeleteTunerMappings(IEnumerable<int> tunerMappingIds)
    {
      _channel.DeleteTunerMappings(tunerMappingIds);
    }

    #endregion

    #endregion

    #region channel-to-group mappings

    public ChannelGroupChannelMapping SaveChannelGroupMapping(ChannelGroupChannelMapping mapping)
    {
      mapping.UnloadAllUnchangedRelationsForEntity();
      return _channel.SaveChannelGroupMapping(mapping);
    }

    public IList<ChannelGroupChannelMapping> SaveChannelGroupMappings(IEnumerable<ChannelGroupChannelMapping> mappings)
    {
      foreach (ChannelGroupChannelMapping mapping in mappings)
      {
        mapping.UnloadAllUnchangedRelationsForEntity();
      }
      return _channel.SaveChannelGroupMappings(mappings);
    }

    public void DeleteChannelGroupMapping(int idChannelGroupMap)
    {
      _channel.DeleteChannelGroupMapping(idChannelGroupMap);
    }

    public void DeleteChannelGroupMappings(IEnumerable<int> channelGroupMappingIds)
    {
      _channel.DeleteChannelGroupMappings(channelGroupMappingIds);
    }

    #endregion
  }
}