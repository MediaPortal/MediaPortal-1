using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ProgramCategoryManagement
  {
    public static ProgramCategory AddCategory(ProgramCategory programCategory)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.Add<ProgramCategory>(programCategory);
        programCategoryRepository.UnitOfWork.SaveChanges();
        programCategory.AcceptChanges();
        return programCategory;
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

    public static ProgramCategory SaveProgramCategory(ProgramCategory programCategory)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.AttachEntityIfChangeTrackingDisabled(programCategoryRepository.ObjectContext.ProgramCategories, programCategory);
        programCategoryRepository.ApplyChanges(programCategoryRepository.ObjectContext.ProgramCategories, programCategory);
        programCategoryRepository.UnitOfWork.SaveChanges();
        programCategory.AcceptChanges();
        return programCategory;
      }
    }

    public static IList<ProgramCategory> SaveProgramCategories(IEnumerable<ProgramCategory> programCategories)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.AttachEntityIfChangeTrackingDisabled(programCategoryRepository.ObjectContext.ProgramCategories, programCategories);
        programCategoryRepository.ApplyChanges(programCategoryRepository.ObjectContext.ProgramCategories, programCategories);
        programCategoryRepository.UnitOfWork.SaveChanges();
        // TODO gibman, AcceptAllChanges() doesn't seem to reset the change trackers
        //programCategoryRepository.ObjectContext.AcceptAllChanges();
        foreach (ProgramCategory category in programCategories)
        {
          category.AcceptChanges();
        }
        return programCategories.ToList();
      }
    }

    public static IList<GuideCategory> ListAllGuideCategories()
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        return programCategoryRepository.GetAll<GuideCategory>().ToList();
      }
    }

    public static GuideCategory SaveGuideCategory(GuideCategory guideCategory)
    {
      using (IProgramCategoryRepository programCategoryRepository = new ProgramCategoryRepository())
      {
        programCategoryRepository.AttachEntityIfChangeTrackingDisabled(programCategoryRepository.ObjectContext.GuideCategories, guideCategory);
        programCategoryRepository.ApplyChanges(programCategoryRepository.ObjectContext.GuideCategories, guideCategory);
        programCategoryRepository.UnitOfWork.SaveChanges();
        guideCategory.AcceptChanges();
        return guideCategory;
      }
    }
  }
}