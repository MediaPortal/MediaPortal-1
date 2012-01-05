#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using MediaPortal.Configuration;
using MediaPortal.Profile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
    } ;

    private class SkinBool
    {
      public string Name;
      public bool Value;
      public Kind Kind;
    } ;

    private static string _skinSettingsFileName = Config.GetFile(Config.Dir.SelectedSkin, "SkinSettings.xml");
    private static Dictionary<int, SkinString> _skinStringSettings = new Dictionary<int, SkinString>();
    private static Dictionary<int, SkinBool> _skinBoolSettings = new Dictionary<int, SkinBool>();
    private static string _loadedSkinSettings = "";

    public const string THEME_SKIN_DEFAULT = "Skin default";
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
      Dictionary<int, SkinString>.Enumerator enumer = _skinStringSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinString skin = enumer.Current.Value;
        if (skin.Name == line)
        {
          return enumer.Current.Key;
        }
      }
      SkinString newString = new SkinString();
      newString.Name = line;
      newString.Value = line;
      newString.Kind = kind;
      int key = _skinStringSettings.Count;
      _skinStringSettings[key] = newString;

      // Create the setting as a property.
      GUIPropertyManager.SetProperty(newString.Name, newString.Value);

      return key;
    }

    /// <summary>
    /// Retrieve a skin string using the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetSkinString(int key)
    {
      SkinString skin = null;
      if (_skinStringSettings.TryGetValue(key, out skin))
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
      SkinString skin = null;
      if (_skinStringSettings.TryGetValue(key, out skin))
      {
        skin.Value = newValue;
        _skinStringSettings[key] = skin;

        // Save the setting as a property.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);

        // Save change to disk immediately.
        Save();
      }
    }

    /// <summary>
    /// Translate the skin boolean, create the skin boolean as a transient value if not found.
    /// </summary>
    /// <param name="setting"></param>
    /// <returns></returns>
    public static int TranslateSkinBool(string setting)
    {
      return TranslateSkinString(setting, Kind.TRANSIENT);
    }

    /// <summary>
    /// Translate the skin boolean, create the skin boolean if not found.
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static int TranslateSkinBool(string setting, Kind kind)
    {
      Dictionary<int, SkinBool>.Enumerator enumer = _skinBoolSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinBool skin = enumer.Current.Value;
        if (skin.Name == setting)
        {
          return enumer.Current.Key;
        }
      }
      SkinBool newBool = new SkinBool();
      newBool.Name = setting;
      newBool.Value = false;
      newBool.Kind = kind;
      int key = _skinBoolSettings.Count;
      _skinBoolSettings[key] = newBool;

      // Create the setting as a property.  The boolean value is converted as a string representation.
      GUIPropertyManager.SetProperty(newBool.Name, newBool.Value.ToString());

      return key;
    }

    /// <summary>
    /// Retrieve a skin boolean using the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool GetSkinBool(int key)
    {
      SkinBool skinBool = null;
      if (_skinBoolSettings.TryGetValue(key, out skinBool))
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
      SkinBool skinBool = null;
      if (_skinBoolSettings.TryGetValue(key, out skinBool))
      {
        skinBool.Value = newValue;
        _skinBoolSettings[key] = skinBool;

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
      Dictionary<int, SkinBool>.Enumerator enumer = _skinBoolSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinBool skin = enumer.Current.Value;
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
      Dictionary<int, SkinBool>.Enumerator enumer = _skinBoolSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinBool skin = enumer.Current.Value;
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
      Dictionary<int, SkinString>.Enumerator enumer = _skinStringSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinString skin = enumer.Current.Value;
        if (skin.Name == setting)
        {
          skin.Value = "";

          // Save the setting as a property if specified as such.
          GUIPropertyManager.SetProperty(skin.Name, skin.Value);
        }
      }
    }

    /// <summary>
    /// Set all the skin strings to empty.  Does not save to disk; call Save() afterward.
    /// </summary>
    public static void ResetAllSkinString()
    {
      Dictionary<int, SkinString>.Enumerator enumer = _skinStringSettings.GetEnumerator();
      while (enumer.MoveNext())
      {
        SkinString skin = enumer.Current.Value;
        skin.Value = "";

        // Save the setting as a property if specified as such.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);
      }
    }

    #region Persistence

    private static void LoadBooleanSettings()
    {
      _skinBoolSettings.Clear();
      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        IDictionary<string, bool> allBooleanSettings = xmlReader.GetSection<bool>("booleansettings");

        if (allBooleanSettings == null)
        {
          return;
        }

        var enumerator = allBooleanSettings.GetEnumerator();
        if (enumerator != null)
        {
          while (enumerator.MoveNext())
          {
            // Create the new boolean setting.
            SkinBool newBool = new SkinBool();
            newBool.Name = enumerator.Current.Key;
            newBool.Value = enumerator.Current.Value;
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
      _skinStringSettings.Clear();
      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        IDictionary<string, string> allStringSettings = xmlReader.GetSection<string>("stringsettings");

        if (allStringSettings == null)
        {
          return;
        }

        var enumerator = allStringSettings.GetEnumerator();
        if (enumerator != null)
        {
          while (enumerator.MoveNext())
          {
            // Create the new string setting.
            SkinString newString = new SkinString();
            newString.Name = enumerator.Current.Key;
            newString.Value = enumerator.Current.Value;
            newString.Kind = Kind.PERSISTENT;

            // Add the setting to the dictionary.
            int key = _skinStringSettings.Count;
            _skinStringSettings[key] = newString;

            // Create the setting as a property.
            GUIPropertyManager.SetProperty(newString.Name, newString.Value);
          }
        }
      }
    }

    private static void LoadDiscreteSettings()
    {
      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        // Theme settings.
        GUIGraphicsContext.Theme = xmlReader.GetValueAsString(THEME_SECTION_NAME, THEME_NAME_ENTRY, "");
        GUIPropertyManager.SetProperty("#skin.currenttheme", GUIGraphicsContext.ThemeName);

        // Set a property with a comma-separated list of theme names.
        string themesCSV = "";
        ArrayList themes = GetSkinThemes();
        for (int i = 0; i < themes.Count; i++)
        {
          themesCSV += "," + themes[i];
        }

        if (themesCSV.Length > 0)
        {
          themesCSV = themesCSV.Substring(1);
        }
        GUIPropertyManager.SetProperty("#skin.themes", themesCSV);
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
          LoadBooleanSettings();
          LoadStringSettings();
          LoadDiscreteSettings();

          _loadedSkinSettings = GUIGraphicsContext.SkinName;
        }
      }
      catch (Exception ex)
      {
        Log.Error("SkinSettings: Error loading settings from {0} - {1}", _skinSettingsFileName, ex.Message);
      }
    }

    /// <summary>
    /// Save all skin settings to disk.
    /// </summary>
    public static void Save()
    {
      using (Settings xmlWriter = new Settings(_skinSettingsFileName))
      {
        SaveBooleanSettings(xmlWriter);
        SaveStringSettings(xmlWriter);
        SaveDiscreteSettings(xmlWriter);
      }
      Settings.SaveCache();
    }

    private static void SaveBooleanSettings(Settings xmlWriter)
    {
      Dictionary<int, SkinBool>.Enumerator bEnumer = _skinBoolSettings.GetEnumerator();
      SkinBool bSetting;
      while (bEnumer.MoveNext())
      {
        bSetting = bEnumer.Current.Value;
        if (bSetting.Kind == Kind.PERSISTENT)
        {
          xmlWriter.SetValue("booleansettings", bSetting.Name, bSetting.Value);
        }
      }
    }

    private static void SaveStringSettings(Settings xmlWriter)
    {
      Dictionary<int, SkinString>.Enumerator strEnumer = _skinStringSettings.GetEnumerator();
      SkinString strSetting;
      while (strEnumer.MoveNext())
      {
        strSetting = strEnumer.Current.Value;
        if (strSetting.Kind == Kind.PERSISTENT)
        {
          xmlWriter.SetValue("stringsettings", strSetting.Name, strSetting.Value);
        }
      }
    }

    private static void SaveDiscreteSettings(Settings xmlWriter)
    {
      // Theme settings
      xmlWriter.SetValue(THEME_SECTION_NAME, THEME_NAME_ENTRY, GUIGraphicsContext.ThemeName);
    }

    /// <summary>
    /// Return a list of available themes for the current skin.  The first entry in the list is always the skin default.
    /// </summary>
    /// <returns></returns>
    public static ArrayList GetSkinThemes()
    {
      ArrayList themes = new ArrayList();

      // Add the skin default (no theme selected).
      themes.Add(THEME_SKIN_DEFAULT);
      try
      {
        string[] themesArray = Directory.GetDirectories(String.Format(@"{0}\Themes", GUIGraphicsContext.Skin), "*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < themesArray.Length; i++)
        {
          themesArray[i] = themesArray[i].Substring(themesArray[i].LastIndexOf(@"\") + 1);
        }
        themes.AddRange(themesArray);
      }
      catch (DirectoryNotFoundException)
      {
        // The Themes directory was not found.  Ignore and return an empty string.
      }
      return themes;
    }

    #endregion

  }
}