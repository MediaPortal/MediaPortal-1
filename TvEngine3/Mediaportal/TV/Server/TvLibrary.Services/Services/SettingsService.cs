using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;

namespace Mediaportal.TV.Server.TVLibrary.Services
{
  public class SettingService : ISettingService
  {
    
    public Setting GetSetting(string tagName)
    {
      return SettingsManagement.GetSetting(tagName);
    }

    public Setting GetSettingWithDefaultValue(string tagName, string defaultValue)
    {
      return SettingsManagement.GetSetting(tagName, defaultValue);
    }

    public void SaveSetting(string tagName, string defaultValue)
    {
      SettingsManagement.SaveSetting(tagName, defaultValue);
    }    
  }
}
