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

using MediaPortal.Profile;
using MediaPortal.Common.Utils;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Xml;

namespace MediaPortal.GUI.Library
{
  public class GUIThemeManager
  {
    public const string THEME_SKIN_DEFAULT = "Skin default";
    private static Thread ActivateThemeThread;
    private static string _themeName = "";
    private static int _focusControlId = 0;

    private static void ThreadActivateTheme()
    {
      GUIWaitCursor.Show();

      // Need to initialize fonts and references if they change based on the theme.
      // Check current theme.
      bool initFonts = (GUIGraphicsContext.HasThemeSpecificSkinFile(@"\fonts.xml"));
      bool initReferences = (GUIGraphicsContext.HasThemeSpecificSkinFile(@"\references.xml"));

      // Change the theme and save this new setting.
      SetTheme(_themeName);
      SkinSettings.Save();

      // Check new theme.
      initFonts = initFonts || GUIGraphicsContext.HasThemeSpecificSkinFile(@"\fonts.xml");
      initReferences = initReferences || GUIGraphicsContext.HasThemeSpecificSkinFile(@"\references.xml");

      // Reset fonts if needed.
      if (initFonts)
      {
        // Reinitializing the device while changing fonts freezes the UI.
        // Add some sleep() to present the wait cursor animation for at least some user feedback.
        Thread.Sleep(500);
        GUIFontManager.ClearFontCache();
        GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
        GUIFontManager.InitializeDeviceObjects();
        Thread.Sleep(500);
      }

      // Force a reload of the control references if needed.
      if (initReferences)
      {
        GUIControlFactory.ClearReferences();
      }

      // Reactivate the current window and refocus on the control used to change the theme.
      // This applies the new theme to the current window immediately.
      GUIWindowManager.ResetAllControls();
      GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow, true, true, _focusControlId);

      GUIWaitCursor.Hide();
    }

    public static string ActivateThemeByName(string themeName, int focusControlId)
    {
      // Change themes in a background thread so that the wait cursor gets presented during the theme change.
      _themeName = themeName;
      _focusControlId = focusControlId;

      ActivateThemeThread = new Thread(new ThreadStart(ThreadActivateTheme));
      ActivateThemeThread.Name = "Activate theme for skin";
      ActivateThemeThread.IsBackground = true;
      ActivateThemeThread.Priority = ThreadPriority.Normal;
      ActivateThemeThread.Start();
      return themeName;
    }

    public static string ActivateThemeNext(int direction, int focusControlId)
    {
      // Switch the next theme in the list; either the next or previous based on the direction.
      // Theme with empty string refers to the skin default (no theme set).
      string skinTheme = CurrentTheme;

      ArrayList themes = GetSkinThemes();
      if (themes.Count > 0)
      {
        int index = themes.IndexOf(skinTheme);
        if (index < 0)
        {
          // Theme is default or not in the list, set to first theme.
          index = 0;
        }
        else
        {
          // Select the next theme.
          index += direction;
        }

        // If backed up past the first theme then select the last theme.
        if (index < 0)
        {
          index = themes.Count - 1;
        }
        else if (index >= themes.Count)
        {
          index = 0;
        }

        skinTheme = themes[index].ToString();
        ActivateThemeByName(skinTheme, focusControlId);
      }
      return skinTheme;
    }

    /// <summary>
    /// Returns the current theme name.
    /// </summary>
    /// <returns></returns>
    public static string CurrentTheme
    {
      get { return GUIGraphicsContext.ThemeName; }
    }

    /// <summary>
    /// Set the current theme
    /// </summary>
    /// <param name="name"></param>
    public static void SetTheme(string name)
    {
      CheckThemeVersion(ref name);
      GUIGraphicsContext.Theme = name;
      GUIPropertyManager.SetProperty("#skin.currenttheme", name);
    }

    /// <summary>
    /// Validates that the specified theme is version compatible with the base skin.
    /// </summary>
    /// <param name="name"></param>
    private static void CheckThemeVersion(ref string name)
    {
      if (THEME_SKIN_DEFAULT.Equals(name))
      {
        return;  // The default theme is the skin itself.
      }

      using (Settings xmlreader = new MPSettings())
      {
        bool ignoreErrors = false;
        ignoreErrors = xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
        if (ignoreErrors)
        {
          return;
        }
      }

      Version versionTheme = null;
      string filename = GUIGraphicsContext.Skin + @"\Themes\" + name + @"\theme.xml";

      if (File.Exists(filename))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(filename);
        XmlNode node = doc.SelectSingleNode("/controls/theme/version");
        if (node != null && node.InnerText != null)
        {
          versionTheme = new Version(node.InnerText);
        }
      }

      // Theme is compatible with the skin if the versions are the same.
      if (CompatibilityManager.SkinVersion.Equals(versionTheme))
      {
        return;
      }

      // Theme is incompatible, force to default theme
      Log.Info("GUIThemeManager: User skin theme is not compatable with base skin, skin={0}({1}) theme={2}({3}).  Using skin default (no theme). ",
        GUIGraphicsContext.SkinName, CompatibilityManager.SkinVersion, name, versionTheme);
      name = THEME_SKIN_DEFAULT;
    }

    /// <summary>
    /// Returns true if the current theme is skin default (no theme set).
    /// </summary>
    public static bool CurrentThemeIsDefault
    {
      get { return THEME_SKIN_DEFAULT.Equals(CurrentTheme); }
    }

    /// <summary>
    /// Initialize ThemeManager, should be called when a new skin is loaded.
    /// </summary>
    public static void Init(string name)
    {
      // Set the current theme.
      SetTheme(name);

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

    /// <summary>
    /// Clears all properties identifying themes.
    /// </summary>
    public static void ClearSettings()
    {
      GUIPropertyManager.RemoveProperty("#skin.currenttheme");
      GUIPropertyManager.RemoveProperty("#skin.themes");
    }

    /// <summary>
    /// Return a list of available themes for the current skin.  The first entry in the list is always the skin default.
    /// </summary>
    /// <returns></returns>
    private static ArrayList GetSkinThemes()
    {
      return GetSkinThemesForSkin(GUIGraphicsContext.Skin);
    }

    /// <summary>
    /// Return a list of available themes for the specified skin folder path.  The first entry in the list is always the skin default.
    /// </summary>
    /// <returns></returns>
    public static ArrayList GetSkinThemesForSkin(string skinPath)
    {
      ArrayList themes = new ArrayList();

      // Add the skin default (no theme selected).
      themes.Add(THEME_SKIN_DEFAULT);
      try
      {
        ArrayList themeCandidates = new ArrayList(Directory.GetDirectories(String.Format(@"{0}\Themes", skinPath), "*", SearchOption.TopDirectoryOnly));
        string themeCandidate;
        string themeFile;
        for (int i = 0; i < themeCandidates.Count; i++)
        {
          // Validate the theme by checking for a "theme.xml" file in the directory.
          themeCandidate = (string)themeCandidates[i];
          themeFile = Path.Combine(themeCandidate, @"theme.xml");
          if (File.Exists(themeFile))
          {
            themes.Add(themeCandidate.Substring(themeCandidate.LastIndexOf(@"\") + 1));
          }
        }
      }
      catch (DirectoryNotFoundException)
      {
        // The Themes directory was not found.  Returns on the skin default.
      }
      return themes;
    }
  }
}
