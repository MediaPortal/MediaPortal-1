using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ITunerRepository : IRepository<Model>
  {
    IQueryable<Tuner> IncludeAllRelations(IQueryable<Tuner> query, TunerRelation includeRelations);
  }
}