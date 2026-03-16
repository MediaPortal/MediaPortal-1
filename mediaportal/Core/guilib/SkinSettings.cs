#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MediaPortal.GUI.Library
{
  public class SkinSettings
  {
    public enum Kind
    {
      TRANSIENT = 0,  // This kind of setting will not be saved to disk.
      PERSISTENT = 1  // This kind of setting will be saved to disk.
    }

    private class SkinString
    {
      public string Name;
      public string Value;
      public Kind Kind;

      public override string ToString()
      {
        return String.Format("{0}:{1}, {2}", Name, Value, Kind);
      }
    }

    private class SkinBool
    {
      public string Name;
      public bool Value;
      public Kind Kind;
      public override string ToString()
      {
        return String.Format("{0}:{1}, {2}", Name, Value, Kind);
      }
    };

    private static Dictionary<int, SkinString> _skinStringSettings = new Dictionary<int, SkinString>();
    private static Dictionary<int, SkinBool> _skinBoolSettings = new Dictionary<int, SkinBool>();
    private static Dictionary<string, int> _propertyLocation = new Dictionary<string, int>();
    private static string _loadedSkinSettings = "";
    private static bool _noTheme;

    public const string THEME_SECTION_NAME = "theme";
    public const string THEME_NAME_ENTRY = "name";

    /// <summary>
    /// Translate the skin string, create the skin string as a transient value if not found.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static int TranslateSkinString(string line)
    {
      return TranslateSkinString(line, Kind.TRANSIENT);
    }

    /// <summary>
    /// Translate the skin string, create the skin string if not found.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static int TranslateSkinString(string line, Kind kind)
    {
      lock (_skinStringSettings)
      {
        if (_propertyLocation.TryGetValue(line, out int iKey))
        {
          SkinString skin = _skinStringSettings[iKey];
          if (skin.Kind == Kind.TRANSIENT && kind == Kind.PERSISTENT)
          {
            skin.Kind = kind;
          }
          return iKey;
        }
      }

      SkinString newString = new SkinString();
      newString.Name = line;
      newString.Value = line;
      newString.Kind = kind;

      // Create the setting as a property if not already present.
      if (!GUIPropertyManager.PropertyIsDefined(newString.Name))
      {
        GUIPropertyManager.SetProperty(newString.Name, newString.Value);
      }
      else
      {
        newString.Value = GUIPropertyManager.GetProperty(newString.Name);
      }

      int key;
      lock (_skinStringSettings) //Lock the dictionary, it might be getting saved at the moment
      {
        key = _skinStringSettings.Count;
        _skinStringSettings[key] = newString;
        _propertyLocation[newString.Name] = key;
      }
      return key;
    }

    /// <summary>
    /// Retrieve a skin setting name using the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetSkinName(int key)
    {
      if (_skinStringSettings.TryGetValue(key, out SkinString skin))
      {
        return skin.Name;
      }
      return "";
    }

    /// <summary>
    /// Retrieve a skin string using the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetSkinString(int key)
    {
      if (_skinStringSettings.TryGetValue(key, out SkinString skin))
      {
        return skin.Value;
      }
      return "";
    }

    /// <summary>
    /// Set a skin string using the specified key.  Saves changes to disk immediatley.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue"></param>
    public static void SetSkinString(int key, string newValue)
    {
      if (_skinStringSettings.TryGetValue(key, out SkinString skin))
      {
        skin.Value = newValue;

        // Save the setting as a property.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);

        // Save change to disk immediately.
        Save();
      }
    }

    public static void PropertyChanged(string tag, string tagValue)
    {
      lock (_skinStringSettings) //Lock the dictionary, it might be getting saved at the moment
      {
        if (_propertyLocation.TryGetValue(tag, out int key))
        {
          _skinStringSettings[key].Value = tagValue;
        }
      }
    }

    /// <summary>
    /// Translate the skin boolean, create the skin boolean as a transient value if not found.
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static int TranslateSkinBool(string setting)
    {
      return TranslateSkinBool(setting, Kind.TRANSIENT);
    }

    /// <summary>
    /// Translate the skin boolean, create the skin boolean if not found.
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static int TranslateSkinBool(string setting, Kind kind)
    {
      lock (_skinBoolSettings)
      {
        foreach (var kv in _skinBoolSettings)
        {
          if (kv.Value.Name == setting)
          {
            if (kv.Value.Kind == Kind.TRANSIENT && kind == Kind.PERSISTENT)
            {
              kv.Value.Kind = kind;
            }
            return kv.Key;
          }
        }
      }

      SkinBool newBool = new SkinBool();
      newBool.Name = setting;
      newBool.Value = false;
      newBool.Kind = kind;

      // Create the setting as a property if not already present.  The boolean value is converted as a string representation.
      if (!GUIPropertyManager.PropertyIsDefined(newBool.Name))
      {
        GUIPropertyManager.SetProperty(newBool.Name, newBool.Value.ToString());
      }
      else
      {
        try
        {
          newBool.Value = bool.Parse(GUIPropertyManager.GetProperty(newBool.Name));
        }
        catch (FormatException ex)
        {
          // Value is set to false.
          Log.Warn("SkinSettings: Boolean setting value is not a valid boolean name={0} value={1} {2}", newBool.Name, newBool.Value, ex.Message);
        }
      }

      int key;
      lock (_skinBoolSettings) // Lock dictionary, we might be saving, should not alter structre
      {
        key = _skinBoolSettings.Count;
        _skinBoolSettings[key] = newBool;
      }
      return key;
    }

    /// <summary>
    /// Retrieve a skin boolean using the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetSkinBool(int key)
    {
      if (_skinBoolSettings.TryGetValue(key, out SkinBool skinBool))
      {
        return skinBool.Value;
      }
      return false;
    }

    /// <summary>
    /// Set a skin boolean using the specified key.  Saves changes to disk immediatley.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue"></param>
    public static void SetSkinBool(int key, bool newValue)
    {
      if (_skinBoolSettings.TryGetValue(key, out SkinBool skinBool))
      {
        skinBool.Value = newValue;

        // Save the setting as a property.  The boolean value is converted as a string representation.
        GUIPropertyManager.SetProperty(skinBool.Name, skinBool.Value.ToString());

        // Save change to disk immediately.
        Save();
      }
    }

    /// <summary>
    /// Set the specified skin boolean to false.  Does not save to disk; call Save() afterward.
    /// </summary>
    /// <param name="setting"></param>
    public static void ResetSkinBool(string setting)
    {
      foreach (var skin in _skinBoolSettings.Values)
      {
        if (skin.Name == setting)
        {
          skin.Value = false;

          // Save the setting as a property if specified as such.  The boolean value is converted as a string representation.
          GUIPropertyManager.SetProperty(skin.Name, skin.Value.ToString());
        }
      }
    }

    /// <summary>
    /// Set all skin booleans to false.  Does not save to disk; call Save() afterward.
    /// </summary>
    public static void ResetAllSkinBool()
    {
      foreach (var skin in _skinBoolSettings.Values)
      {
        skin.Value = false;

        // Save the setting as a property if specified as such.  The boolean value is converted as a string representation.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value.ToString());
      }
    }

    /// <summary>
    /// Set the specified skin string to empty.  Does not save to disk; call Save() afterward.
    /// </summary>
    /// <param name="setting"></param>
    public static void ResetSkinString(string setting)
    {
      if (_propertyLocation.TryGetValue(setting, out int key))
      {
        SkinString skin = _skinStringSettings[key];
        skin.Value = "";

        // Save the setting as a property if specified as such.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);
      }
    }

    /// <summary>
    /// Set all the skin strings to empty.  Does not save to disk; call Save() afterward.
    /// </summary>
    public static void ResetAllSkinString()
    {
      foreach (var skin in _skinStringSettings.Values)
      {
        skin.Value = "";

        // Save the setting as a property if specified as such.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);
      }
    }

    /// <summary>
    /// Set the skin to Notheme for watchdog.
    /// </summary>
    public static bool NoTheme
    {
      get { return _noTheme; }
      set { _noTheme = value; }
    }

    #region Persistence

    private static void LoadBooleanSettings()
    {
      using (Settings xmlReader = new SKSettings())
      {
        IDictionary<string, bool> allBooleanSettings = xmlReader.GetSection<bool>("booleansettings");

        if (allBooleanSettings == null)
        {
          return;
        }

        lock (_skinBoolSettings)
        {
          foreach (var kv in allBooleanSettings)
          {
            // Create the new boolean setting.
            SkinBool newBool = new SkinBool();
            newBool.Name = kv.Key;
            newBool.Value = kv.Value;
            newBool.Kind = Kind.PERSISTENT;

            // Add the setting to the dictionary.
            int key = _skinBoolSettings.Count;
            _skinBoolSettings[key] = newBool;

            // Create the setting as a property.  The boolean value is converted as a string representation.
            GUIPropertyManager.SetProperty(newBool.Name, newBool.Value.ToString());
          }
        }
      }
    }

    private static void LoadStringSettings()
    {
      using (Settings xmlReader = new SKSettings())
      {
        IDictionary<string, string> allStringSettings = xmlReader.GetSection<string>("stringsettings");

        if (allStringSettings == null)
        {
          return;
        }

        lock (_skinStringSettings)
        {
          foreach (var kv in allStringSettings)
          {
            // Create the new string setting.
            SkinString newString = new SkinString();
            newString.Name = kv.Key;
            newString.Value = kv.Value;
            newString.Kind = Kind.PERSISTENT;

            // Add the setting to the dictionary.
            int key = _skinStringSettings.Count;
            _skinStringSettings[key] = newString;
            _propertyLocation[newString.Name] = key;

            // Create the setting as a property.
            GUIPropertyManager.SetProperty(newString.Name, newString.Value);
          }
        }
      }
    }

    private static void LoadDiscreteSettings()
    {
      using (Settings xmlReader = new SKSettings())
      {
        if (!_noTheme)
        {
          // Initialize the theme manager for the selected theme.
          GUIThemeManager.Init(xmlReader.GetValueAsString(THEME_SECTION_NAME, THEME_NAME_ENTRY, GUIThemeManager.THEME_SKIN_DEFAULT));
        }
        else
        {
          // Initialize the theme manager for the watchdog.
          GUIThemeManager.Init(GUIThemeManager.THEME_SKIN_DEFAULT);
        }
      }
    }

    /// <summary>
    /// Load all skin settings from disk.  Loads only once after the skin has changed.
    /// </summary>
    public static void Load()
    {
      try
      {
        if (_loadedSkinSettings != GUIGraphicsContext.SkinName)
        {
          // Prior to loading skin settings we need to unload any existing settings; especially inthe case when we are changing skins.
          ClearAllSettings();

          LoadBooleanSettings();
          LoadStringSettings();
          LoadDiscreteSettings();

          _loadedSkinSettings = GUIGraphicsContext.SkinName;
        }
      }
      catch (Exception ex)
      {
        Log.Error("SkinSettings: Error loading settings from {0} - {1}", SKSettings.ConfigPathName, ex.Message);
      }
    }

    private static void ClearAllSettings()
    {
      ClearBooleanSettings();
      ClearStringSettings();
      ClearDiscreteSettings();
    }

    private static void ClearBooleanSettings()
    {
      List<int> keysToRemove = new List<int>();
      foreach (var kv in _skinBoolSettings)
      {
        SkinBool bSetting = kv.Value;
        if (bSetting.Kind == Kind.PERSISTENT) // Keep transient settings
        {
          GUIPropertyManager.RemoveProperty(bSetting.Name);
          keysToRemove.Add(kv.Key);
        }
      }

      // Clear our dictionary.
      lock (_skinBoolSettings)
      {
        foreach (int key in keysToRemove)
        {
          _skinBoolSettings.Remove(key);
        }
      }
    }

    private static void ClearStringSettings()
    {
      List<int> keysToRemove = new List<int>();
      foreach (var kv in _skinStringSettings)
      {
        SkinString strSetting = kv.Value;
        if (strSetting.Kind == Kind.PERSISTENT) // Keep transient settings
        {
          GUIPropertyManager.RemoveProperty(strSetting.Name);
          keysToRemove.Add(kv.Key);
          _propertyLocation.Remove(strSetting.Name);
        }
      }

      // Clear our dictionary.
      lock (_skinStringSettings)
      {
        foreach (int key in keysToRemove)
        {
          _skinStringSettings.Remove(key);
        }
      }
    }

    private static void ClearDiscreteSettings()
    {
      GUIThemeManager.ClearSettings();
    }

    static IWork _delaySave;
    /// <summary>
    /// Schedule saving of all settings in the near future
    /// </summary>
    public static void Save()
    {
      IThreadPool tp = GlobalServiceProvider.Get<IThreadPool>();
      if (_delaySave == null)
      {
        _delaySave = tp.Add(LazySave, "Wait for saving SkinSettings");
      }
      else if (_delaySave.State != WorkState.INPROGRESS && _delaySave.State != WorkState.INQUEUE)
      {
        _delaySave = tp.Add(LazySave, "Wait for saving SkinSettings");
      }

    }

    static void LazySave()
    {
      System.Threading.Thread.Sleep(100); // This combines quick calls to Save into one Save operation
      IThreadPool tp = GlobalServiceProvider.Get<IThreadPool>();
      tp.Add(_Save); // Add the save operation to the thread pool
    }

    /// <summary>
    /// Save all skin settings to disk.
    /// </summary>
    public static void _Save()
    {
      using (Settings xmlWriter = new SKSettings())
      {
        lock (_skinBoolSettings)
        {
          SaveBooleanSettings(xmlWriter);
        }
        lock (_skinStringSettings)
        {
          SaveStringSettings(xmlWriter);
        }
        SaveDiscreteSettings(xmlWriter);
      }
      Settings.SaveCache();
      Log.Debug("SkinSettings: Saved all settings.");
    }

    private static void SaveBooleanSettings(Settings xmlWriter)
    {
      foreach (var bSetting in _skinBoolSettings.Values)
      {
        if (bSetting.Kind == Kind.PERSISTENT)
        {
          xmlWriter.SetValue("booleansettings", bSetting.Name, bSetting.Value);
        }
      }
    }

    private static void SaveStringSettings(Settings xmlWriter)
    {
      foreach (var strSetting in _skinBoolSettings.Values)
      {
        if (strSetting.Kind == Kind.PERSISTENT)
        {
          xmlWriter.SetValue("stringsettings", strSetting.Name, strSetting.Value);
        }
      }
    }

    private static void SaveDiscreteSettings(Settings xmlWriter)
    {
      // Theme settings
      xmlWriter.SetValue(THEME_SECTION_NAME, THEME_NAME_ENTRY, GUIThemeManager.CurrentTheme);
    }

    #endregion

  }
}