using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  [ServiceKnownType(typeof(ChannelAnalogTv))]
  [ServiceKnownType(typeof(ChannelAtsc))]
  [ServiceKnownType(typeof(ChannelCapture))]
  [ServiceKnownType(typeof(ChannelDigiCipher2))]
  [ServiceKnownType(typeof(ChannelDvbC))]
  [ServiceKnownType(typeof(ChannelDvbC2))]
  [ServiceKnownType(typeof(ChannelDvbS))]
  [ServiceKnownType(typeof(ChannelDvbS2))]
  [ServiceKnownType(typeof(ChannelDvbT))]
  [ServiceKnownType(typeof(ChannelDvbT2))]
  [ServiceKnownType(typeof(ChannelFmRadio))]
  [ServiceKnownType(typeof(ChannelSatelliteTurboFec))]
  [ServiceKnownType(typeof(ChannelScte))]
  [ServiceKnownType(typeof(ChannelStream))]
  [ServiceKnownType(typeof(LnbTypeBLL))]
  public interface IChannelService
  {
    [OperationContract]
    IList<Channel> ListAllChannels();

    [OperationContract(Name = "ListAllChannelsWithSpecificRelations")]
    IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<Channel> ListAllChannelsByGroupId(int groupId);

    [OperationContract(Name = "ListAllChannelsByGroupIdWithSpecificRelations")]
    IList<Channel> ListAllChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<Channel> ListAllVisibleChannelsByGroupId(int groupId);

    [OperationContract(Name = "ListAllVisibleChannelsByGroupIdWithSpecificRelations")]
    IList<Channel> ListAllVisibleChannelsByGroupId(int groupId, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<Channel> ListAllChannelsByMediaType(MediaType mediaType);

    [OperationContract(Name = "ListAllChannelsByMediaTypeWithSpecificRelations")]
    IList<Channel> ListAllChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType);

    [OperationContract(Name = "ListAllVisibleChannelsByMediaTypeWithSpecificRelations")]
    IList<Channel> ListAllVisibleChannelsByMediaType(MediaType mediaType, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    Channel GetChannel(int idChannel);

    [OperationContract(Name = "GetChannelWithSpecificRelations")]
    Channel GetChannel(int idChannel, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<Channel> GetChannelsByName(string channelName);

    [OperationContract]
    IList<Channel> GetChannelsByName(string channelName, ChannelIncludeRelationEnum includeRelations);

    [OperationContract]
    Channel SaveChannel(Channel channel);

    [OperationContract]
    IList<Channel> SaveChannels(IEnumerable<Channel> channels);

    [OperationContract]
    void DeleteChannel(int idChannel);

    [OperationContract]
    Channel MergeChannels(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations);

    #region tuning details

    [OperationContract]
    IList<TuningDetail> ListAllTuningDetailsByChannel(int idChannel);

    [OperationContract]
    TuningDetail GetTuningDetail(int idTuningDetail);

    [OperationContract]
    IList<TuningDetail> GetAnalogTelevisionTuningDetails(int physicalChannelNumber);

    [OperationContract]
    IList<TuningDetail> GetAtscScteTuningDetails(BroadcastStandard broadcastStandard, string logicalChannelNumber, int? frequency = null);

    [OperationContract]
    IList<TuningDetail> GetCaptureTuningDetails(string name);

    [OperationContract]
    IList<TuningDetail> GetDvbTuningDetails(BroadcastStandard broadcastStandard, int originalNetworkId, int serviceId, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetFmRadioTuningDetails(int frequency);

    [OperationContract]
    IList<TuningDetail> GetFreesatTuningDetails(int channelId);

    [OperationContract]
    IList<TuningDetail> GetMpeg2TuningDetails(BroadcastStandard broadcastStandard, int programNumber, int? transportStreamId = null, int? frequency = null, int? satelliteId = null);

    [OperationContract]
    IList<TuningDetail> GetOpenTvTuningDetails(int channelId);

    [OperationContract]
    IList<TuningDetail> GetStreamTuningDetails(string url);

    [OperationContract]
    void AddTuningDetail(int idChannel, IChannel channel);

    [OperationContract]
    void UpdateTuningDetail(int idChannel, int idTuningDetail, IChannel channel);

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