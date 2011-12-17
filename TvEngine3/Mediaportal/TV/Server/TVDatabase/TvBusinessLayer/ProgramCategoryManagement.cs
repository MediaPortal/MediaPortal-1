using System.Collections.Generic;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class ProgramCategoryManagement
  {

    public static void AddCategory(ProgramCategory category)
    {
      using (var programCategoryRepository = new GenericRepository<Model>())
      {
        programCategoryRepository.Add<ProgramCategory>(category);
        programCategoryRepository.UnitOfWork.SaveChanges();
      }
    }

    public static IList<ProgramCategory> ListAllProgramCategories()
    {
      using (var programCategoryRepository = new GenericRepository<Model>())
      {
        return programCategoryRepository.GetAll<ProgramCategory>().ToList();
      }
    }
  }
}
