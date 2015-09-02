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
    public static void SaveValue(string tagName, int defaultValue)
    {
      SaveValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, double defaultValue)
    {
      SaveValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, bool defaultValue)
    {
      SaveValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }
    
    public static void SaveValue(string tagName, string defaultValue)
    {
      using (ISettingRepository settingsRepository = new SettingRepository(true))
      {
        Setting setting = settingsRepository.SaveSetting(tagName, defaultValue);
        EntityCacheHelper.Instance.SettingCache.AddOrUpdateCache(tagName, setting);
      }
    }
    
    public static void SaveValue(string tagName, DateTime defaultValue)
    {
      SaveValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture));
    }

    public static int GetValue(string tagName, int defaultValue)
    {
      int number;
      if (!int.TryParse(GetValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture)), out number))
      {
        number = defaultValue;
      }
      return number;
    }

    public static double GetValue(string tagName, double defaultValue)
    {
      double number;      
      if (!double.TryParse(GetValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture)), out number))
      {
        number = defaultValue;
      }
      return number;
    }

    public static bool GetValue(string tagName, bool defaultValue)
    {
      return GetValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture)) == true.ToString(CultureInfo.InvariantCulture);
    }

    public static string GetValue(string tagName, string defaultValue)
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
      return setting.Value;
    }

    public static DateTime GetValue(string tagName, DateTime defaultValue)
    {
      DateTime dateTime;
      if (!DateTime.TryParse(GetValue(tagName, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
      {
        dateTime = defaultValue;
      }
      return dateTime;
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