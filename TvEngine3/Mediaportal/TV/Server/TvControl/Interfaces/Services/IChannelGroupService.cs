using System.Collections.Generic;
using System.ServiceModel;
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
    IList<ChannelGroup> ListAllCustomChannelGroups(ChannelGroupIncludeRelationEnum includeRelations, MediaTypeEnum mediaType);

    [OperationContract(Name = "ListAllChannelGroupsByMediaTypeWithSpecificRelations")]
    IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType, ChannelGroupIncludeRelationEnum includeRelations);

    [OperationContract]    
    IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaTypeEnum mediaType);

    [OperationContract]    
    ChannelGroup GetChannelGroupByNameAndMediaType(string groupName, MediaTypeEnum mediaType);
    
    [OperationContract]
    ChannelGroup GetOrCreateGroup(string groupName, MediaTypeEnum mediaType);

    

    [OperationContract]
    void DeleteChannelGroupMap(int idMap);
    [OperationContract]
    ChannelGroup GetChannelGroup(int id);
    [OperationContract]
    ChannelGroup SaveGroup(ChannelGroup @group);
    [OperationContract]
    void DeleteChannelGroup(int idGroup);    
        
  }
}
