using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface IProgramCategoryRepository : IRepository<Model>
  {
    IQueryable<ProgramCategory> IncludeAllRelations(IQueryable<ProgramCategory> query);
  }
}