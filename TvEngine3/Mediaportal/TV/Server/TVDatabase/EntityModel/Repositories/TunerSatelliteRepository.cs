using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TunerSatelliteRepository : GenericRepository<Model>, ITunerSatelliteRepository
  {
    public TunerSatelliteRepository()
    {
    }

    public TunerSatelliteRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TunerSatelliteRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<TunerSatellite> IncludeAllRelations(IQueryable<TunerSatellite> query, TunerSatelliteRelation includeRelations)
    {
      if (includeRelations.HasFlag(TunerSatelliteRelation.Satellite))
      {
        query.Include(ts => ts.Satellite);
      }
      if (includeRelations.HasFlag(TunerSatelliteRelation.LnbType))
      {
        query.Include(ts => ts.LnbType);
      }
      if (includeRelations.HasFlag(TunerSatelliteRelation.Tuner))
      {
        query.Include(ts => ts.Tuner);
      }
      return query;
    }
  }
}