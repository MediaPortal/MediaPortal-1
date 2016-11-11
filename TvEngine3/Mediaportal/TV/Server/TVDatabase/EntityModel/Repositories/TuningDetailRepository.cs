using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TuningDetailRepository : GenericRepository<Model>, ITuningDetailRepository
  {
    public TuningDetailRepository()
    {
    }

    public TuningDetailRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TuningDetailRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<TuningDetail> IncludeAllRelations(IQueryable<TuningDetail> query, TuningDetailRelation includeRelations)
    {
      if (includeRelations.HasFlag(TuningDetailRelation.Channel))
      {
        query = query.Include(td => td.Channel);
      }
      if (includeRelations.HasFlag(TuningDetailRelation.Satellite))
      {
        query = query.Include(td => td.Satellite);
      }
      if (includeRelations.HasFlag(TuningDetailRelation.TunerMappings))
      {
        query = query.Include(td => td.TunerMappings);
      }
      return query;
    }
  }
}