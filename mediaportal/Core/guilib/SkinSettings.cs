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

    private static string _skinSettingsFileName = Config.GetFile(Config.Dir.Config, "SkinSettings.xml");
    private static Dictionary<int, SkinString> _skinStringSettings = new Dictionary<int, SkinString>();
    private static Dictionary<int, SkinBool> _skinBoolSettings = new Dictionary<int, SkinBool>();
    private static string _loadedSkinSettings = "";

    public static int TranslateSkinString(string line)
    {
      return TranslateSkinString(line, Kind.TRANSIENT);
    }

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

      // Create the setting as a property if specified as such.
      GUIPropertyManager.SetProperty(newString.Name, newString.Value);

      return key;
    }

    public static string GetSkinString(int key)
    {
      SkinString skin = null;
      if (_skinStringSettings.TryGetValue(key, out skin))
      {
        return skin.Value;
      }
      return "";
    }

    public static void SetSkinString(int key, string newValue)
    {
      SkinString skin = null;
      if (_skinStringSettings.TryGetValue(key, out skin))
      {
        skin.Value = newValue;
        _skinStringSettings[key] = skin;

        // Save the setting as a property if specified as such.
        GUIPropertyManager.SetProperty(skin.Name, skin.Value);
      }
    }

    public static int TranslateSkinBool(string setting)
    {
      return TranslateSkinString(setting, Kind.TRANSIENT);
    }

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

      // Create the setting as a property if specified as such.  The boolean value is converted as a string representation.
      GUIPropertyManager.SetProperty(newBool.Name, newBool.Value.ToString());

      return key;
    }

    public static bool GetSkinBool(int key)
    {
      SkinBool skinBool = null;
      if (_skinBoolSettings.TryGetValue(key, out skinBool))
      {
        return skinBool.Value;
      }
      return false;
    }

    public static void SetSkinBool(int key, bool newValue)
    {
      SkinBool skinBool = null;
      if (_skinBoolSettings.TryGetValue(key, out skinBool))
      {
        skinBool.Value = newValue;
        _skinBoolSettings[key] = skinBool;

        // Save the setting as a property if specified as such.  The boolean value is converted as a string representation.
        GUIPropertyManager.SetProperty(skinBool.Name, skinBool.Value.ToString());
      }
    }

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

    private static void LoadBooleanSettings()
    {
      _skinBoolSettings.Clear();

      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        bool success = false;
        string allSettings = xmlReader.GetValueAsString(GUIGraphicsContext.SkinName, "BooleanSettings", "");

        // Each pair is separated by a semicolon.
        string[] pairs = allSettings.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);

        foreach (string setting in pairs)
        {
          // Each name/value pair is separated by an equal sign.
          string[] nv = setting.Split('=');

          // Create the new boolean setting.
          SkinBool newBool = new SkinBool();
          newBool.Name = nv[0];
          newBool.Value = bool.TryParse(nv[1], out success);
          if (!success)
          {
            newBool.Value = false;
          }
          newBool.Kind = Kind.PERSISTENT;

          // Add the setting to th dictionary.
          int key = _skinBoolSettings.Count;
          _skinBoolSettings[key] = newBool;
        }
      }
    }

    private static void LoadStringSettings()
    {
      _skinStringSettings.Clear();

      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        string allSettings = xmlReader.GetValueAsString(GUIGraphicsContext.SkinName, "StringSettings", "");

        // Each pair is separated by a semicolon.
        string[] pairs = allSettings.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);

        foreach (string setting in pairs)
        {
          // Each name/value pair is separated by an equal sign.
          string[] nv = setting.Split('=');

          // Create the new string setting.
          SkinString newString = new SkinString();
          newString.Name = nv[0];
          newString.Value = nv[1];
          newString.Kind = Kind.PERSISTENT;

          // Add the setting to th dictionary.
          int key = _skinStringSettings.Count;
          _skinStringSettings[key] = newString;
        }
      }
    }

    private static void LoadDiscreteSettings()
    {
      using (Settings xmlReader = new Settings(_skinSettingsFileName))
      {
        string skinTheme = xmlReader.GetValueAsString(GUIGraphicsContext.SkinName, "Theme", "");
        GUIGraphicsContext.Theme = skinTheme;
        GUIPropertyManager.SetProperty("#Skin.CurrentTheme", GUIGraphicsContext.ThemeName);

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
        GUIPropertyManager.SetProperty("#Skin.Themes", themesCSV);
      }
    }

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

    public static void Save()
    {
      using (Settings xmlWriter = new Settings(_skinSettingsFileName))
      {
        // Save all the boolean settings.
        string allSettings = "";
        Dictionary<int, SkinBool>.Enumerator bEnumer = _skinBoolSettings.GetEnumerator();
        SkinBool bSetting;
        while (bEnumer.MoveNext())
        {
          bSetting = bEnumer.Current.Value;
          if (bSetting.Kind == Kind.PERSISTENT)
          {
            allSettings += bSetting.Name + "=" + bSetting.Value + ";";
          }
        }
        xmlWriter.SetValue(GUIGraphicsContext.SkinName, "BooleanSettings", allSettings);

        // Save all the string settings.
        allSettings = "";
        Dictionary<int, SkinString>.Enumerator strEnumer = _skinStringSettings.GetEnumerator();
        SkinString strSetting;
        while (strEnumer.MoveNext())
        {
          strSetting = strEnumer.Current.Value;
          if (strSetting.Kind == Kind.PERSISTENT)
          {
            allSettings += strSetting.Name + "=" + strSetting.Value + ";";
          }
        }
        xmlWriter.SetValue(GUIGraphicsContext.SkinName, "StringSettings", allSettings);

        // Save discrete settings.
        xmlWriter.SetValue(GUIGraphicsContext.SkinName, "Theme", GUIGraphicsContext.ThemeName);
      }
      Settings.SaveCache();
    }

    public static ArrayList GetSkinThemes()
    {
      ArrayList themes = new ArrayList();
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
  }
}