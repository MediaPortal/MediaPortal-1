using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ProgramCategoryRepository : GenericRepository<Model>, IProgramCategoryRepository
  {
    public ProgramCategoryRepository()
    {
    }

    public ProgramCategoryRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }


    public ProgramCategoryRepository(Model context)
      : base(context)
    {
    }

    public IQueryable<ProgramCategory> IncludeAllRelations(IQueryable<ProgramCategory> query)
    {
      var includeRelations = query.Include(p => p.TvGuideCategory);
      return includeRelations;
    }
  }
}
