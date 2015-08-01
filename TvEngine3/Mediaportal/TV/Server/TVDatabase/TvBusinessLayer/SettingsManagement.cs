using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Repositories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class SettingsManagement
  {
    public static Setting GetSetting(string tagName)
    {
      Setting setting = EntityCacheHelper.Instance.SettingCache.GetOrUpdateFromCache(tagName,
                  delegate
                    {
                      using (ISettingRepository settingsRepository = new SettingRepository())
                      {
                        return settingsRepository.GetSetting(tagName);
                      }
                    }
        );
      return setting;
    }

    public static void SaveSetting(string tagName, string value)
    {            
      using (ISettingRepository settingsRepository = new SettingRepository(true))
      {
        Setting setting = settingsRepository.SaveSetting(tagName, value);
        EntityCacheHelper.Instance.SettingCache.AddOrUpdateCache(tagName, setting);
      }      
    }

    public static void SaveValue(string tagName, int defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, double defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, bool defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
    }
    
    public static void SaveValue(string tagName, string defaultValue)
    {
      SaveSetting(tagName, defaultValue);
    }
    
    public static void SaveValue(string tagName, DateTime defaultValue)
    {
      SaveSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static Setting GetSetting(string tagName, string defaultValue)
    {
      Setting setting = EntityCacheHelper.Instance.SettingCache.GetFromCache(tagName);

      if (setting == null)
      {
        using (ISettingRepository settingsRepository = new SettingRepository(true))
        {
          setting = settingsRepository.GetOrSaveSetting(tagName, defaultValue);
          EntityCacheHelper.Instance.SettingCache.AddOrUpdateCache(tagName, setting);
        } 
      }
      return setting;
    }

    public static int GetValue(string tagName, int defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      int number;
      bool parsed = int.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static double GetValue(string tagName, double defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      double number;      
      bool parsed = double.TryParse(setting.Value, out number);
      if (!parsed)
      {
        number = defaultValue;
      }
      return number;
    }

    public static bool GetValue(string tagName, bool defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
      return setting.Value == "true";
    }

    public static string GetValue(string tagName, string defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue);
      return setting.Value;
    }

    public static DateTime GetValue(string tagName, DateTime defaultValue)
    {
      Setting setting = GetSetting(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
      return string.IsNullOrEmpty(setting.Value) ? DateTime.MinValue : DateTime.Parse(setting.Value, CultureInfo.InvariantCulture);
    }

    public static IList<Setting> ListAllSettings()
    {
      using (ISettingRepository settingsRepository = new SettingRepository(true))
      {
        return settingsRepository.GetAll<Setting>().ToList();        
      }
    }
  }
}