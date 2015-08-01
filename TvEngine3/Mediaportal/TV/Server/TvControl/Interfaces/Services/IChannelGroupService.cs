using System.Collections.Generic;
using System.ServiceModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVControl.Interfaces.Services
{
  [ServiceContract(Namespace = "http://www.team-mediaportal.com")]
  public interface IChannelGroupService
  {
    [OperationContract]
    IList<ChannelGroup> ListAllChannelGroups();

    [OperationContract(Name = "ListAllChannelGroupsWithSpecificRelations")]
    IList<ChannelGroup> ListAllChannelGroups(ChannelGroupIncludeRelationEnum includeRelations);

    [OperationContract]
    IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType);

    [OperationContract(Name = "ListAllChannelGroupsByMediaTypeWithSpecificRelations")]
    IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupIncludeRelationEnum includeRelations);

    [OperationContract]
    ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaType mediaType);

    [OperationContract]
    ChannelGroup GetOrCreateGroup(string groupName, MediaType mediaType);

    [OperationContract]
    ChannelGroup GetChannelGroup(int id);

    [OperationContract(Name = "GetChannelGroupWithSpecificRelations")]
    ChannelGroup GetChannelGroup(int id, ChannelGroupIncludeRelationEnum includeRelations);

    [OperationContract]
    ChannelGroup SaveGroup(ChannelGroup @group);

    [OperationContract]
    void DeleteChannelGroup(int idGroup);
  }
}