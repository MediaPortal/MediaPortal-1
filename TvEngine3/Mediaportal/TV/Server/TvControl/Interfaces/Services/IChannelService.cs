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
    Channel MergeChannels(IEnumerable<Channel> channels, ChannelRelation includeRelations);

    #region tuning details

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> ListAllDigitalTransmitterTuningDetails();

    [OperationContract]
    TuningDetail GetTuningDetail(int idTuningDetail, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, TuningDetailRelation includeRelations, int? frequency = null);

    [OperationContract]
    IList<TuningDetail> GetCaptureTuningDetails(string name, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetFmRadioTuningDetails(int frequency, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetFreesatTuningDetails(int channelId, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, TuningDetailRelation includeRelations, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetOpenTvTuningDetails(int channelId, TuningDetailRelation includeRelations);

    [OperationContract]
    IList<TuningDetail> GetStreamTuningDetails(string url, TuningDetailRelation includeRelations);

    [OperationContract]
    void UpdateTuningDetailEpgInfo(TuningDetail transmitterTuningDetail);

    [OperationContract]
    TuningDetail SaveTuningDetail(TuningDetail tuningDetail);

    [OperationContract]
    void DeleteTuningDetail(int idTuningDetail);

    #endregion

    #region channel-to-tuner maps

    [OperationContract]
    ChannelMap SaveChannelMap(ChannelMap channelMap);

    [OperationContract]
    IList<ChannelMap> SaveChannelMaps(IEnumerable<ChannelMap> channelMaps);

    [OperationContract]
    void DeleteChannelMap(int idChannelMap);

    [OperationContract]
    void DeleteChannelMaps(IEnumerable<int> channelMapIds);

    #endregion

    #region channel-to-group maps

    [OperationContract]
    GroupMap SaveChannelGroupMap(GroupMap groupMap);

    [OperationContract]
    IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps);

    [OperationContract]
    void DeleteChannelGroupMap(int idGroupMap);

    [OperationContract]
    void DeleteChannelGroupMaps(IEnumerable<int> groupMapIds);

    #endregion
  }
}