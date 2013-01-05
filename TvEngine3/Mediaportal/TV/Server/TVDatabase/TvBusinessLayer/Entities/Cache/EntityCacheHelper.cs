using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache
{
  public class EntityCacheHelper : Singleton<EntityCacheHelper>
  {
    private EntityCache<ProgramCategory, string> _programCategoryCache;

    public EntityCache<ProgramCategory, string> ProgramCategoryCache
    {
      get
      {
        if (_programCategoryCache == null)
        {
          IList<ProgramCategory> allCats = ProgramCategoryManagement.ListAllProgramCategories();
          var cache = allCats.ToDictionary(programCategory => programCategory.Category);
          _programCategoryCache = new EntityCache<ProgramCategory, string>(cache);
        }
        return _programCategoryCache;
      }
    }
  }
}