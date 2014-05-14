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

    public IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query)
    {
      var includeRelations = query.
        Include(r => r.GroupMaps.Select(c => c.Channel.TuningDetails)).
        Include(r => r.GroupMaps).
        Include(r => r.KeywordMap).
        Include(r => r.GroupMaps.Select(c => c.Channel));
      return includeRelations;
    }

    public IQueryable<ChannelGroup> IncludeAllRelations(IQueryable<ChannelGroup> query, ChannelGroupIncludeRelationEnum includeRelations)
    {
      bool groupMaps = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMaps);
      bool groupMapsChannel = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMapsChannel);
      bool groupMapsTuningDetails = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.GroupMapsTuningDetails);
      bool keywordMap = includeRelations.HasFlag(ChannelGroupIncludeRelationEnum.KeywordMap);

      if (groupMaps)
      {
        query = query.Include(r => r.GroupMaps);
      }

      if (keywordMap)
      {
        query = query.Include(r => r.KeywordMap);
      }

      if (groupMapsChannel)
      {
        query = query.Include(r => r.GroupMaps.Select(c => c.Channel));
      }

      if (groupMapsTuningDetails)
      {
        query = query.Include(r => r.GroupMaps.Select(c => c.Channel.TuningDetails));
      }

      return query;
    }
  }
}
