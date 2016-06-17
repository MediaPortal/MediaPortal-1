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
    IList<ChannelGroup> ListAllChannelGroups(ChannelGroupRelation includeRelations);

    [OperationContract]
    IList<ChannelGroup> ListAllChannelGroupsByMediaType(MediaType mediaType, ChannelGroupRelation includeRelations);

    [OperationContract]
    ChannelGroup GetChannelGroup(int idChannelGroup, ChannelGroupRelation includeRelations);

    [OperationContract]
    ChannelGroup GetOrCreateChannelGroup(string name, MediaType mediaType);

    [OperationContract]
    ChannelGroup SaveChannelGroup(ChannelGroup channelGroup);

    [OperationContract]
    void DeleteChannelGroup(int idChannelGroup);
  }
}