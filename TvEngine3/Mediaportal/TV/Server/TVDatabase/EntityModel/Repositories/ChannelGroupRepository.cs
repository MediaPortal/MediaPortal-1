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
      if (includeRelations.HasFlag(ChannelGroupRelation.ChannelMappings))
      {
        query = query.Include(g => g.ChannelMappings);
      }
      if (includeRelations.HasFlag(ChannelGroupRelation.ChannelMappingsChannel))
      {
        query = query.Include(g => g.ChannelMappings.Select(c => c.Channel));
      }
      if (includeRelations.HasFlag(ChannelGroupRelation.ChannelMappingsTuningDetails))
      {
        query = query.Include(g => g.ChannelMappings.Select(c => c.Channel.TuningDetails));
      }
      return query;
    }
  }
}