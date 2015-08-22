using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class AnalogTunerSettingsRepository : GenericRepository<Model>, IAnalogTunerSettingsRepository
  {
    public AnalogTunerSettingsRepository()
    {
    }

    public AnalogTunerSettingsRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    public AnalogTunerSettingsRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<AnalogTunerSettings> IncludeAllRelations(IQueryable<AnalogTunerSettings> query)
    {
      IQueryable<AnalogTunerSettings> includeRelations =
        query.
          Include(s => s.VideoEncoder).
          Include(s => s.AudioEncoder);
      return includeRelations;
    }
  }
}