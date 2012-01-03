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

using System;
using System.Collections;
using System.Threading;

namespace MediaPortal.GUI.Library
{
  class GUIThemeManager
  {
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
      GUIGraphicsContext.Theme = _themeName;
      GUIPropertyManager.SetProperty("#Skin.CurrentTheme", GUIGraphicsContext.ThemeName);
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

    public static void ActivateThemeByName(string themeName, int focusControlId)
    {
      // Change themes in a background thread so that the wait cursor gets presented during the theme change.
      _themeName = themeName;
      _focusControlId = focusControlId;

      ActivateThemeThread = new Thread(new ThreadStart(ThreadActivateTheme));
      ActivateThemeThread.Name = "Activate theme for skin";
      ActivateThemeThread.IsBackground = true;
      ActivateThemeThread.Priority = ThreadPriority.Normal;
      ActivateThemeThread.Start();
    }

    public static void ActivateThemeNext(int direction, int focusControlId)
    {
      // Switch the next theme in the list; either the next or previous based on the direction.
      // Theme with empty string refers to the skin default (no theme set).
      string skinTheme = GUIGraphicsContext.ThemeName;

      ArrayList themes = SkinSettings.GetSkinThemes();
      if (themes.Count > 0)
      {
        int index = themes.IndexOf((string)skinTheme);
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

        ActivateThemeByName((string)themes[index], focusControlId);
      }
    }
  }
}
