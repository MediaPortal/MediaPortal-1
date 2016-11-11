using System.Collections.Generic;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ChannelService : IChannelService
  {
    public IList<Channel> ListAllChannels(ChannelRelation includeRelations)
    {
      return ChannelManagement.ListAllChannels(includeRelations);
    }

    public IList<Channel> ListAllChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      return ChannelManagement.ListAllChannelsByGroupId(idChannelGroup, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByGroupId(idChannelGroup, includeRelations);
    }

    public IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      return ChannelManagement.ListAllChannelsByMediaType(mediaType, includeRelations);
    }

    public IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations)
    {
      return ChannelManagement.ListAllVisibleChannelsByMediaType(mediaType, includeRelations);
    }

    public Channel GetChannel(int idChannel, ChannelRelation includeRelations)
    {
      return ChannelManagement.GetChannel(idChannel, includeRelations);
    }

    public IList<Channel> GetChannelsByName(string name, ChannelRelation includeRelations)
    {
      return ChannelManagement.GetChannelsByName(name, includeRelations);
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

    public void DeleteOrphanedChannels(IEnumerable<int> channelIds = null)
    {
      ChannelManagement.DeleteOrphanedChannels(channelIds);
    }

    public Channel MergeChannels(IEnumerable<Channel> channels, ChannelRelation includeRelations)
    {
      return ChannelManagement.MergeChannels(channels, includeRelations);
    }

    #region tuning details

    public IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.ListAllTuningDetailsByChannel(idChannel, includeRelations);
    }

    public IList<TuningDetail> ListAllTuningDetailsByBroadcastStandard(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? idSatellite = null)
    {
      return TuningDetailManagement.ListAllTuningDetailsByBroadcastStandard(broadcastStandards, includeRelations, idSatellite);
    }

    public IList<TuningDetail> ListAllTuningDetailsByMediaType(MediaType mediaType, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.ListAllTuningDetailsByMediaType(mediaType, includeRelations);
    }

    public IList<TuningDetail> ListAllTuningDetailsByOriginalNetworkIds(IEnumerable<int> originalNetworkIds, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.ListAllTuningDetailsByOriginalNetworkIds(originalNetworkIds, includeRelations);
    }

    public IList<TuningDetail> ListAllDigitalTransmitterTuningDetails()
    {
      return TuningDetailManagement.ListAllDigitalTransmitterTuningDetails();
    }

    public TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetTuningDetail(idTuningDetail, includeRelations);
    }

    public IList<TuningDetail> GetAmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetAmRadioTuningDetails(frequency, includeRelations);
    }

    public IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetAnalogTelevisionTuningDetails(physicalChannelNumber, includeRelations);
    }

    public IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandards, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null)
    {
      return TuningDetailManagement.GetAtscScteTuningDetails(broadcastStandards, logicalChannelNumber, includeRelations, frequency);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetCaptureTuningDetails(name, includeRelations);
    }

    public IList<TuningDetail> GetCaptureTuningDetails(int tunerId, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetCaptureTuningDetails(tunerId, includeRelations);
    }

    public IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandards, int originalNetworkId, TuningDetailRelation includeRelations, int? serviceId = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return TuningDetailManagement.GetDvbTuningDetails(broadcastStandards, originalNetworkId, includeRelations, serviceId, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetExternalTunerTuningDetails(TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetExternalTunerTuningDetails(includeRelations);
    }

    public IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetFmRadioTuningDetails(frequency, includeRelations);
    }

    public IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetFreesatTuningDetails(channelId, includeRelations);
    }

    public IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? programNumber = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null)
    {
      return TuningDetailManagement.GetMpeg2TsTuningDetails(broadcastStandards, includeRelations, programNumber, transportStreamId, frequency, satelliteId);
    }

    public IList<TuningDetail> GetOpenTvTuningDetails(BroadcastStandard broadcastStandards, int channelId, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetOpenTvTuningDetails(broadcastStandards, channelId, includeRelations);
    }

    public IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations)
    {
      return TuningDetailManagement.GetStreamTuningDetails(url, includeRelations);
    }

    public void UpdateTuningDetailEpgInfo(TuningDetail transmitterTuningDetail)
    {
      TuningDetailManagement.UpdateTuningDetailEpgInfo(transmitterTuningDetail);
    }

    public TuningDetail SaveTuningDetail(TuningDetail tuningDetail)
    {
      return TuningDetailManagement.SaveTuningDetail(tuningDetail);
    }

    public void DeleteTuningDetail(int idTuning)
    {
      TuningDetailManagement.DeleteTuningDetail(idTuning);
    }

    #region tuning-detail-to-tuner mappings

    public IList<TunerTuningDetailMapping> ListAllTunerMappings()
    {
      return TuningDetailManagement.ListAllTunerMappings();
    }

    public TunerTuningDetailMapping SaveTunerMapping(TunerTuningDetailMapping mapping)
    {
      return TuningDetailManagement.SaveTunerMapping(mapping);
    }

    public IList<TunerTuningDetailMapping> SaveTunerMappings(IEnumerable<TunerTuningDetailMapping> mappings)
    {
      return TuningDetailManagement.SaveTunerMappings(mappings);
    }

    public void DeleteTunerMapping(int idTunerMapping)
    {
      TuningDetailManagement.DeleteTunerMapping(idTunerMapping);
    }

    public void DeleteTunerMappings(IEnumerable<int> tunerMappingIds)
    {
      TuningDetailManagement.DeleteTunerMappings(tunerMappingIds);
    }

    #endregion

    #endregion

    #region channel-to-group mappings

    public ChannelGroupChannelMapping SaveChannelGroupMapping(ChannelGroupChannelMapping mapping)
    {
      return ChannelManagement.SaveChannelGroupMapping(mapping);
    }

    public IList<ChannelGroupChannelMapping> SaveChannelGroupMappings(IEnumerable<ChannelGroupChannelMapping> mappings)
    {
      return ChannelManagement.SaveChannelGroupMappings(mappings);
    }

    public void DeleteChannelGroupMapping(int idChannelGroupMapping)
    {
      ChannelManagement.DeleteChannelGroupMapping(idChannelGroupMapping);
    }

    public void DeleteChannelGroupMappings(IEnumerable<int> channelGroupMappingIds)
    {
      ChannelManagement.DeleteChannelGroupMaps(channelGroupMappingIds);
    }

    #endregion
  }
}