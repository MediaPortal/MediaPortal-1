using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  // Define a service contract.  
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  [ServiceKnownType(typeof(DVBTChannel))]
  [ServiceKnownType(typeof(DVBCChannel))]
  [ServiceKnownType(typeof(DVBSChannel))]
  [ServiceKnownType(typeof(ATSCChannel))]
  [ServiceKnownType(typeof(DVBIPChannel))]
  [ServiceKnownType(typeof(AnalogChannel))]
  public interface IChannelService
  {
    [OperationContract]    
    IList<Channel> GetAllChannelsByGroupIdAndMediaType(int groupId, MediaTypeEnum mediatype);

    [OperationContract]    
    IList<Channel> GetAllChannelsByGroupId(int groupId);

    [OperationContract]    
    IList<Channel> ListAllChannels();

    [OperationContract]
    IList<Channel> SaveChannels(IEnumerable<Channel> channels);

    [OperationContract]
    IList<GroupMap> SaveChannelGroupMaps(IEnumerable<GroupMap> groupMaps);

    [OperationContract]
    IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType);

    [OperationContract]
    IList<Channel> GetChannelsByName(string channelName);

    [OperationContract]
    Channel SaveChannel(Channel channel);

    [OperationContract]
    Channel GetChannel(int idChannel);

    [OperationContract]
    void DeleteChannel(int idChannel);

    [OperationContract]
    TuningDetail SaveTuningDetail(TuningDetail tuningDetail);

    [OperationContract]
    IList<Channel> ListAllVisibleChannelsByMediaType(MediaTypeEnum mediaType);

    [OperationContract]
    TuningDetail GetTuningDetail(DVBBaseChannel dvbChannel);

    [OperationContract]
    TuningDetail GetTuningDetailCustom(DVBBaseChannel dvbChannel, TuningDetailSearchEnum tuningDetailSearchEnum);

    [OperationContract]
    TuningDetail GetTuningDetailByURL(DVBBaseChannel dvbChannel, string url);

    [OperationContract]
    void AddTuningDetail(int idChannel, IChannel channel);

    [OperationContract]
    IList<TuningDetail> GetTuningDetailsByName(string channelName, int channelType);

    [OperationContract]
    void UpdateTuningDetail(int idChannel, int idTuning, IChannel channel);    

    [OperationContract]
    Channel GetChannelByName(string channelName, ChannelIncludeRelationEnum includeRelations);
    [OperationContract]
    void DeleteTuningDetail(int idTuning);
    [OperationContract]
    GroupMap SaveChannelGroupMap(GroupMap groupMap);
    [OperationContract]
    void DeleteChannelMap(int idChannelMap);
    [OperationContract]
    ChannelMap SaveChannelMap(ChannelMap map);

    [OperationContract(Name = "ListAllChannelsWithSpecificRelations")]
    IList<Channel> ListAllChannels(ChannelIncludeRelationEnum includeRelations);

    [OperationContract(Name = "ListAllChannelsByMediaTypeWithSpecificRelations")]
    IList<Channel> ListAllChannelsByMediaType(MediaTypeEnum mediaType, ChannelIncludeRelationEnum includeRelations);

    [OperationContract(Name = "GetAllChannelsByGroupIdAndMediaTypeWithSpecificRelations")]
    IList<Channel> GetAllChannelsByGroupIdAndMediaType(int idGroup, MediaTypeEnum mediaTypeEnum, ChannelIncludeRelationEnum include);    
  } 
}
