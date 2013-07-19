using System;
using System.Data.Entity;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class ProgramCategoryRepository : GenericRepository<TvModel>, IProgramCategoryRepository
  {

    public ProgramCategoryRepository()
    {
    }

    public ProgramCategoryRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }


    public ProgramCategoryRepository(TvModel context)
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
