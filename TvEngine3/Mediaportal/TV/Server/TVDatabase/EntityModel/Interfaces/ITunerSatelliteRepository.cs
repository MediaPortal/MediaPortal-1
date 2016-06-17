using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ITunerSatelliteRepository : IRepository<Model>
  {
    IQueryable<TunerSatellite> IncludeAllRelations(IQueryable<TunerSatellite> query, TunerSatelliteRelation includeRelations);
  }
}