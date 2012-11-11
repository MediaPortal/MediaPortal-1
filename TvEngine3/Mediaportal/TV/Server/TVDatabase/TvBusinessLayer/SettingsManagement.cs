using System;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SettingsManagement
  {    
    public static Setting GetSetting(string tagName)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository())
      {
        return settingsRepository.GetSetting(tagName);
      }
    }

    public static void SaveSetting(string tagName, string value)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository(true))
      {
        settingsRepository.SaveSetting(tagName, value);
      }
    }

    public static Setting GetSetting(string tagName, string defaultValue)
    {
      using (ISettingsRepository settingsRepository = new SettingsRepository(true))
      {
        return settingsRepository.GetOrSaveSetting(tagName, defaultValue);
      }
    }

    // maximum hours to keep old program info
    private static int _epgKeepDuration;
    public static int EpgKeepDuration
    {
      get
      {
        if (_epgKeepDuration == 0)
        {
          // first time query settings, caching
          Setting duration = GetSetting("epgKeepDuration", "24");          
          _epgKeepDuration = Convert.ToInt32(duration.Value);
        }
        return _epgKeepDuration;
      }
    }

  }
}
