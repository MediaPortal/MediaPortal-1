using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories
{
  public class SettingRepository : GenericRepository<Model>, ISettingRepository
  {
    public SettingRepository()
    {
    }

    public SettingRepository(Model context)
      : base(context)
    {
    }

    public SettingRepository(bool trackingEnabled)
      : base(trackingEnabled)
    {
    }

    /// <summary>
    /// saves a value to the database table "Setting"
    /// </summary>
    public Setting SaveSetting(string tagName, string value)
    {
      Setting setting = First<Setting>(s => s.Tag == tagName);
      if (setting == null)
      {
        setting = new Setting { Value = value, Tag = tagName };
        Add(setting);
      }
      else
      {
        setting.Value = value;
      }

      UnitOfWork.SaveChanges();
      setting.AcceptChanges();
      return setting;
    }

    public Setting GetOrSaveSetting(string tagName, string defaultValue)
    {
      if (defaultValue == null)
      {
        return null;
      }
      if (string.IsNullOrEmpty(tagName))
      {
        return null;
      }

      Setting setting = First<Setting>(s => s.Tag == tagName);
      if (setting == null)
      {
        setting = new Setting { Value = defaultValue, Tag = tagName };
        Add(setting);
        UnitOfWork.SaveChanges();
      }

      return setting;
    }

    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesnt exist a empty string will be the value</returns>
    public Setting GetSetting(string tagName)
    {
      var setting = GetOrSaveSetting(tagName, string.Empty);
      return setting;
    }
  }
}
