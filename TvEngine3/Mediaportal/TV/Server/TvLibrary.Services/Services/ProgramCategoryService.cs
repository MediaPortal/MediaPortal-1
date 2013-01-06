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
      var listAllProgramCategories = ProgramCategoryManagement.ListAllProgramCategories();
      return listAllProgramCategories;
    }

    public IList<TvGuideCategory> ListAllTvGuideCategories()
    {
      var listAllProgramCategories = ProgramCategoryManagement.ListAllTvGuideCategories();
      return listAllProgramCategories;
    }

    public TvGuideCategory SaveTvGuideCategory(TvGuideCategory tvGuideCategory)
    {
      return ProgramCategoryManagement.AddTvGuideCategory(tvGuideCategory);      
    }

    public ProgramCategory SaveProgramCategory(ProgramCategory programCategory)
    {
      return ProgramCategoryManagement.SaveProgramCategory(programCategory);
    }

  }
}
