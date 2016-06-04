using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IChannelRepository : IRepository<Model>
  {
    IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query);
    IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelIncludeRelationEnum includeRelations);
    IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelIncludeRelationEnum includeRelations);
    Channel LoadNavigationProperties(Channel channel, ChannelIncludeRelationEnum includeRelations);
    Channel LoadNavigationProperties(Channel channel);
    IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels);
  }
}