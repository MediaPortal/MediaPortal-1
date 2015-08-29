#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Settings
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public sealed class GUISettingsGeneralStartup : GUIInternalWindow
  {
    [SkinControl(10)] private readonly GUICheckButton _cmStartfullscreen = null;
    [SkinControl(11)] private readonly GUICheckButton _cmUsefullscreensplash = null;
    [SkinControl(11)] private readonly GUICheckButton _cmkeepstartfullscreen = null;
    [SkinControl(12)] private readonly GUICheckButton _cmAlwaysontop = null;
    [SkinControl(13)] private readonly GUICheckButton _cmHidetaskbar = null;
    [SkinControl(14)] private readonly GUICheckButton _cmAutostart = null;
    [SkinControl(15)] private readonly GUICheckButton _cmMinimizeonstartup = null;
    [SkinControl(16)] private readonly GUICheckButton _cmMinimizeonexit = null;
    [SkinControl(17)] private readonly GUICheckButton _cmMinimizeonfocusloss = null;

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
        _cmStartfullscreen.Selected = xmlreader.GetValueAsBool("general", "startfullscreen", true);
        _cmUsefullscreensplash.Selected = xmlreader.GetValueAsBool("general", "usefullscreensplash", true);
        _cmkeepstartfullscreen.Selected = xmlreader.GetValueAsBool("general", "keepstartfullscreen", false);
        _cmAlwaysontop.Selected = xmlreader.GetValueAsBool("general", "alwaysontop", false);
        _cmHidetaskbar.Selected = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        _cmAutostart.Selected = xmlreader.GetValueAsBool("general", "autostart", false);
        _cmMinimizeonstartup.Selected = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
        _cmMinimizeonexit.Selected = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
        _cmMinimizeonfocusloss.Selected = xmlreader.GetValueAsBool("general", "minimizeonfocusloss", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("general", "startfullscreen", _cmStartfullscreen.Selected);
        xmlwriter.SetValueAsBool("general", "usefullscreensplash", _cmUsefullscreensplash.Selected);
        xmlwriter.SetValueAsBool("general", "keepstartfullscreen", _cmkeepstartfullscreen.Selected);
        xmlwriter.SetValueAsBool("general", "alwaysontop", _cmAlwaysontop.Selected);
        try
        {
          if (_cmAlwaysontop.Selected) // always on top
          {
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
              if (subkey != null) subkey.SetValue("ForegroundLockTimeout", 0);
            }
          }
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch (Exception) { }
        // ReSharper restore EmptyGeneralCatchClause

        xmlwriter.SetValueAsBool("general", "hidetaskbar", _cmHidetaskbar.Selected);
        xmlwriter.SetValueAsBool("general", "autostart", _cmAutostart.Selected);
        try
        {
          if (_cmAutostart.Selected) // autostart on boot
          {
        Log.Debug("AUTOSTART");
            string fileName = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
              if (subkey != null)
              {
                subkey.SetValue("MediaPortal", fileName);
              }
            }
          }
          else
          {
            using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
              if (subkey != null)
              {
                subkey.DeleteValue("MediaPortal", false);
              }
            }
          }
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch (Exception) { }
        // ReSharper restore EmptyGeneralCatchClause

        xmlwriter.SetValueAsBool("general", "minimizeonstartup", _cmMinimizeonstartup.Selected);
        xmlwriter.SetValueAsBool("general", "minimizeonexit", _cmMinimizeonexit.Selected);
        xmlwriter.SetValueAsBool("general", "minimizeonfocusloss", _cmMinimizeonfocusloss.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      LoadSettings();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101019)); //General - Startup
      base.OnPageLoad();

      if (!Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (GUISettings.IsPinLocked() && !GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (GUISettings.SettingsChanged && !Util.Utils.IsGUISettingsWindow(newWindowId))
      {
        GUISettings.OnRestartMP(GetID);
      }

      base.OnPageDestroy(newWindowId);
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
      if (control == _cmStartfullscreen)
      {
        SettingsChanged(true);
      }
      if (control == _cmUsefullscreensplash)
      {
        SettingsChanged(true);
      }
      if (control == _cmkeepstartfullscreen)
      {
        SettingsChanged(true);
      }
      if (control == _cmAlwaysontop)
      {
        SettingsChanged(true);
      }
      if (control == _cmHidetaskbar)
      {
        SettingsChanged(true);
      }
      if (control == _cmAutostart)
      {
        SettingsChanged(true);
      }
      if (control == _cmMinimizeonstartup)
      {
        SettingsChanged(true);
      }
      if (control == _cmMinimizeonexit)
      {
        SettingsChanged(true);
      }
      if (control == _cmMinimizeonfocusloss)
      {
        SettingsChanged(true);
      }
     
      base.OnClicked(controlId, control, actionType);
    }

    #endregion
    
    private void SettingsChanged(bool settingsChanged)
    {
      GUISettings.SettingsChanged = settingsChanged;
    }

  }
}