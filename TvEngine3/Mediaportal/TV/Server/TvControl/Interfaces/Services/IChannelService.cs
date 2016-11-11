using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IChannelService
  {
    [OperationContract]
    IList<Channel> ListAllChannels(ChannelRelation includeRelations);

    [OperationContract]
    IList<Channel> ListAllChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations);

    [OperationContract]
    IList<Channel> ListAllVisibleChannelsByGroupId(int idChannelGroup, ChannelRelation includeRelations);

    [OperationContract]
    IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations);

    [OperationContract]
    IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelRelation includeRelations);

    [OperationContract]
    Channel GetChannel(int idChannel, ChannelRelation includeRelations);

    [OperationContract]
    IList<Channel> GetChannelsByName(string name, ChannelRelation includeRelations);

    [OperationContract]
    Channel SaveChannel(Channel channel);

    [OperationContract]
    IList<Channel> SaveChannels(IEnumerable<Channel> channels);

    [OperationContract]
    void DeleteChannel(int idChannel);

    [OperationContract]
    void DeleteOrphanedChannels(IEnumerable<int> channelIds = null);

    [OperationContract]
    Channel MergeChannels(IEnumerable<Channel> channels, ChannelRelation includeRelations);

    #region tuning details

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByBroadcastStandard(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? idSatellite = null);

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByMediaType(MediaType mediaType, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByOriginalNetworkIds(IEnumerable<int> originalNetworkIds, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> ListAllDigitalTransmitterTuningDetails();

    [OperationContract]
    TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetAmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandards, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null);

    [OperationContract]
    IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations);

    [OperationContract(Name = "GetCaptureTuningDetailsByTunerId")]
    IList<TuningDetail> GetCaptureTuningDetails(int tunerId, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandards, int originalNetworkId, TuningDetailRelation includeRelations, int? serviceId = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetExternalTunerTuningDetails(TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandards, TuningDetailRelation includeRelations, int? programNumber = null, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetOpenTvTuningDetails(BroadcastStandard broadcastStandards, int channelId, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations);

    [OperationContract]
    void UpdateTuningDetailEpgInfo(TuningDetail transmitterTuningDetail);

    [OperationContract]
    TuningDetail SaveTuningDetail(TuningDetail tuningDetail);

    [OperationContract]
    void DeleteTuningDetail(int idTuningDetail);

    #region tuning-detail-to-tuner mappings

    [OperationContract]
    IList<TunerTuningDetailMapping> ListAllTunerMappings();

    [OperationContract]
    TunerTuningDetailMapping SaveTunerMapping(TunerTuningDetailMapping mapping);

    [OperationContract]
    IList<TunerTuningDetailMapping> SaveTunerMappings(IEnumerable<TunerTuningDetailMapping> mappings);

    [OperationContract]
    void DeleteTunerMapping(int idTunerMapping);

    [OperationContract]
    void DeleteTunerMappings(IEnumerable<int> tunerMappingIds);

    #endregion

    #endregion

    #region channel-to-group mappings

    [OperationContract]
    ChannelGroupChannelMapping SaveChannelGroupMapping(ChannelGroupChannelMapping mapping);

    [OperationContract]
    IList<ChannelGroupChannelMapping> SaveChannelGroupMappings(IEnumerable<ChannelGroupChannelMapping> mapping);

    [OperationContract]
    void DeleteChannelGroupMapping(int idChannelGroupMapping);

    [OperationContract]
    void DeleteChannelGroupMappings(IEnumerable<int> channelGroupMappingIds);

    #endregion
  }
}