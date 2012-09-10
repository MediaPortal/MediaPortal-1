using System.Collections.Generic;
using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Services
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
