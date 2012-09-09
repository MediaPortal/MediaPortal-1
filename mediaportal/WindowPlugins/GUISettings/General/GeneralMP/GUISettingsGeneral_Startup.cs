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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class GUISettingsGeneralStartup : GUIInternalWindow
  {
    [SkinControl(10)] protected GUICheckButton cmStartfullscreen = null;
    [SkinControl(11)] protected GUICheckButton cmUsefullscreensplash = null;
    [SkinControl(12)] protected GUICheckButton cmAlwaysontop = null;
    [SkinControl(13)] protected GUICheckButton cmHidetaskbar = null;
    [SkinControl(14)] protected GUICheckButton cmAutostart = null;
    [SkinControl(15)] protected GUICheckButton cmMinimizeonstartup = null;
    [SkinControl(16)] protected GUICheckButton cmMinimizeonexit = null;

    public GUISettingsGeneralStartup()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALSTARTUP; //1019
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General_Startup.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // startup settings
        cmStartfullscreen.Selected = xmlreader.GetValueAsBool("general", "startfullscreen", true);
        cmUsefullscreensplash.Selected = xmlreader.GetValueAsBool("general", "usefullscreensplash", true);
        cmAlwaysontop.Selected = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        cmHidetaskbar.Selected = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        cmAutostart.Selected = xmlreader.GetValueAsBool("general", "autostart", false);
        cmMinimizeonstartup.Selected = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
        cmMinimizeonexit.Selected = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "startfullscreen", cmStartfullscreen.Selected);
        xmlwriter.SetValueAsBool("general", "usefullscreensplash", cmUsefullscreensplash.Selected);
        xmlwriter.SetValueAsBool("general", "alwaysontop", cmAlwaysontop.Selected);
        try
        {
          if (cmAlwaysontop.Selected) // always on top
          {
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
              if (subkey != null) subkey.SetValue("ForegroundLockTimeout", 0);
            }
          }
        }
        catch (Exception) { }

        xmlwriter.SetValueAsBool("general", "hidetaskbar", cmHidetaskbar.Selected);
        xmlwriter.SetValueAsBool("general", "autostart", cmAutostart.Selected);
        try
        {
          if (cmAutostart.Selected) // autostart on boot
          {
            string fileName = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
            using (
              RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",
                                                                   true)
              )
            {
              if (subkey != null) subkey.SetValue("MediaPortal", fileName);
            }
          }
          else
          {
            using (
              RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run",
                                                                   true)
              )
            {
              if (subkey != null) subkey.DeleteValue("MediaPortal", false);
            }
          }
        }
        catch (Exception) { }

        xmlwriter.SetValueAsBool("general", "minimizeonstartup", cmMinimizeonstartup.Selected);
        xmlwriter.SetValueAsBool("general", "minimizeonexit", cmMinimizeonexit.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101019)); //General - Startup
      base.OnPageLoad();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      SaveSettings();
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      base.OnAction(action);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      // Startup/Resume
      if (control == cmStartfullscreen)
      {
        SettingsChanged(true);
      }
      if (control == cmUsefullscreensplash)
      {
        SettingsChanged(true);
      }
      if (control == cmAlwaysontop)
      {
        SettingsChanged(true);
      }
      if (control == cmHidetaskbar)
      {
        SettingsChanged(true);
      }
      if (control == cmAutostart)
      {
        SettingsChanged(true);
      }
      if (control == cmMinimizeonstartup)
      {
        SettingsChanged(true);
      }
      if (control == cmMinimizeonexit)
      {
        SettingsChanged(true);
      }
     
      base.OnClicked(controlId, control, actionType);
    }

    #endregion
    
    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }

  }
}