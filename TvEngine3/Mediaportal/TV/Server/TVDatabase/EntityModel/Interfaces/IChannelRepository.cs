using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IChannelRepository : IRepository<Model>
  {
    IQueryable<Channel> IncludeAllRelations(IQueryable<Channel> query, ChannelRelation includeRelations);
    IList<Channel> LoadNavigationProperties(IEnumerable<Channel> channels, ChannelRelation includeRelations);
    Channel LoadNavigationProperties(Channel channel, ChannelRelation includeRelations);
  }
}