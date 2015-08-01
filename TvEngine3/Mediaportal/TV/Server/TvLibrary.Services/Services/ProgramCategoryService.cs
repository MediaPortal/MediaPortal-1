using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class ProgramCategoryService : IProgramCategoryService
  {
    public IList<ProgramCategory> ListAllProgramCategories()
    {
      return ProgramCategoryManagement.ListAllProgramCategories();
    }

    public ProgramCategory SaveProgramCategory(ProgramCategory programCategory)
    {
      return ProgramCategoryManagement.SaveProgramCategory(programCategory);
    }

    public IList<ProgramCategory> SaveProgramCategories(IEnumerable<ProgramCategory> programCategories)
    {
      return ProgramCategoryManagement.SaveProgramCategories(programCategories);
    }

    public IList<GuideCategory> ListAllGuideCategories()
    {
      return ProgramCategoryManagement.ListAllGuideCategories();
    }

    public GuideCategory SaveGuideCategory(GuideCategory guideCategory)
    {
      return ProgramCategoryManagement.SaveGuideCategory(guideCategory);      
    }
  }
}