using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ITunerGroupRepository : IRepository<Model>
  {
    IQueryable<TunerGroup> IncludeAllRelations(IQueryable<TunerGroup> query);
  }
}