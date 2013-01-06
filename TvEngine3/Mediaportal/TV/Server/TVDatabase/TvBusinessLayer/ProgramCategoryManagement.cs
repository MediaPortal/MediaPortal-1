using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ProgramCategoryManagement
  {

    public static void AddCategory(ProgramCategory category)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.Add<ProgramCategory>(category);
        programCategoryRepository.UnitOfWork.SaveChanges();
      }
    }

    public static IList<ProgramCategory> ListAllProgramCategories()
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        IQueryable<ProgramCategory> query = programCategoryRepository.GetAll<ProgramCategory>();
        query = programCategoryRepository.IncludeAllRelations(query);
        return query.ToList();

      }
    }

    public static ProgramCategory SaveProgramCategory(ProgramCategory category)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.AttachEntityIfChangeTrackingDisabled(programCategoryRepository.ObjectContext.ProgramCategories, category);
        programCategoryRepository.ApplyChanges(programCategoryRepository.ObjectContext.ProgramCategories, category);
        programCategoryRepository.UnitOfWork.SaveChanges();
        category.AcceptChanges();
        return category;
      }
    }

    public static TvGuideCategory AddTvGuideCategory(TvGuideCategory tvGuideCategorycategory)
    {
      using (var programCategoryRepository = new GenericRepository<Model>())
      {
        programCategoryRepository.Add<TvGuideCategory>(tvGuideCategorycategory);
        programCategoryRepository.UnitOfWork.SaveChanges();
        return tvGuideCategorycategory;
      }
    }

    public static IList<TvGuideCategory> ListAllTvGuideCategories()
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        IQueryable<TvGuideCategory> query = programCategoryRepository.GetAll<TvGuideCategory>();        
        return query.ToList();
      }
    }
  }
}
