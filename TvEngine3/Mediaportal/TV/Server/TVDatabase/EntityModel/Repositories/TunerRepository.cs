using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class TunerRepository : GenericRepository<Model>, ITunerRepository
  {
    public TunerRepository()
    {
    }

    public TunerRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public TunerRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<Tuner> IncludeAllRelations(IQueryable<Tuner> query, TunerRelation includeRelations)
    {
      if (includeRelations.HasFlag(TunerRelation.ChannelMaps))
      {
        query = query.Include(t => t.ChannelMaps);
      }
      if (includeRelations.HasFlag(TunerRelation.TunerGroup))
      {
        query = query.Include(t => t.TunerGroup);
      }
      if (includeRelations.HasFlag(TunerRelation.TunerProperties))
      {
        query = query.Include(t => t.TunerProperties);
      }
      if (includeRelations.HasFlag(TunerRelation.AnalogTunerSettings))
      {
        query = query.Include(t => t.AnalogTunerSettings).
                      Include(t => t.AnalogTunerSettings.VideoEncoder).
                      Include(t => t.AnalogTunerSettings.AudioEncoder);
      }
      if (includeRelations.HasFlag(TunerRelation.TunerSatellites))
      {
        query = query.Include(t => t.TunerSatellites.Select(ts => ts.Satellite));
      }
      return query;
    }
  }
}