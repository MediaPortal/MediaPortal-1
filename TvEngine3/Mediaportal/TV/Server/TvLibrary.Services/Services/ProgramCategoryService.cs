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
  }
}
