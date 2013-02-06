using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache
{
  public class EntityCacheHelper : Singleton<EntityCacheHelper>
  {
    private EntityCache<ProgramCategory, string> _programCategoryCache;
    private EntityCache<Setting, string> _settingCache;

    public EntityCache<ProgramCategory, string> ProgramCategoryCache
    {
      get
      {
        if (_programCategoryCache == null)
        {
          IList<ProgramCategory> allCats = ProgramCategoryManagement.ListAllProgramCategories();
          Dictionary<string, ProgramCategory> cache = allCats.ToDictionary(programCategory => programCategory.Category);
          _programCategoryCache = new EntityCache<ProgramCategory, string>(cache);
        }
        return _programCategoryCache;
      }
    }

    public EntityCache<Setting, string> SettingCache
    {
      get
      {
        if (_settingCache == null)
        {
          IList<Setting> allSettings = SettingsManagement.ListAllSettings();
          Dictionary<string, Setting> cache = allSettings.ToDictionary(setting => setting.Tag);
          _settingCache = new EntityCache<Setting, string>(cache);
        }
        return _settingCache;
      }
    }
  }
}