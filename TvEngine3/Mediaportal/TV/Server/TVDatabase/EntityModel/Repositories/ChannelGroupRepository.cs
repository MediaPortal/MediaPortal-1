using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ChannelGroupRepository : GenericRepository<Model>, IChannelGroupRepository
  {
    public ChannelGroupRepository()
    {
    }

    public ChannelGroupRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public ChannelGroupRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query, ChannelGroupRelation includeRelations)
    {
      if (includeRelations.HasFlag(ChannelGroupRelation.GroupMaps))
      {
        query = query.Include(g => g.GroupMaps);
      }
      if (includeRelations.HasFlag(ChannelGroupRelation.GroupMapsChannel))
      {
        query = query.Include(g => g.GroupMaps.Select(c => c.Channel));
      }
      if (includeRelations.HasFlag(ChannelGroupRelation.GroupMapsTuningDetails))
      {
        query = query.Include(g => g.GroupMaps.Select(c => c.Channel.TuningDetails));
      }
      return query;
    }
  }
}