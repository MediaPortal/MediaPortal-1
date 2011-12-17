using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces
{
  public interface ISettingsRepository : IRepository<Model>
  {
    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesnt exist the given default string will be the value</returns>
    void SaveSetting(string tagName, string value);

    Setting GetOrSaveSetting(string tagName, string defaultValue);

    /// <summary>
    /// gets a value from the database table "Setting"
    /// </summary>
    /// <returns>A Setting object with the stored value, if it doesnt exist a empty string will be the value</returns>
    Setting GetSetting(string tagName);
  }
}