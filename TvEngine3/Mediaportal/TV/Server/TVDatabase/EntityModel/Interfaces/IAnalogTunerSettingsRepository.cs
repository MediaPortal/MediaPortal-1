using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IAnalogTunerSettingsRepository : IRepository<Model>
  {
    IQueryable<AnalogTunerSettings> IncludeAllRelations(IQueryable<AnalogTunerSettings> query);
  }
}