using Mediaportal.TV.Server.TVControl.Interfaces;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVService.Interfaces.Services;

namespace Mediaportal.TV.Server.TVService.Services
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
