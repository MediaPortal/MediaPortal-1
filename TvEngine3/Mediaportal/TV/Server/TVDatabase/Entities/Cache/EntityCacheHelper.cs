using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Cache
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
          _programCategoryCache = new EntityCache<ProgramCategory, string>();
        }
        return _programCategoryCache;
      }
    }
  }
}