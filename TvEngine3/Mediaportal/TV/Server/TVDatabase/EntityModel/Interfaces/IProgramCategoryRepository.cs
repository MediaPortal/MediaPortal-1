using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IProgramCategoryRepository : IRepository<TvModel>
  {
    IQueryable<ProgramCategory> IncludeAllRelations(IQueryable<ProgramCategory> query);
  }
}