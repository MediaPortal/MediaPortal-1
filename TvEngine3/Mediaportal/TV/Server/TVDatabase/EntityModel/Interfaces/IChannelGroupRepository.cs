using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IChannelGroupRepository : IRepository<Model>
  {
    IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query, ChannelGroupRelation includeRelations);
  }
}