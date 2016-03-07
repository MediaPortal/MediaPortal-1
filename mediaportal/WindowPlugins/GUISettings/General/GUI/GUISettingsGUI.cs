﻿#region Copyright (C) 2005-2011 Team MediaPortal

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

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings;
using MediaPortal.Profile;
using MediaPortal.Util;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGUIMain: GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnGeneral = null;
    [SkinControl(3)] protected GUIButtonControl btnSkin= null;
    [SkinControl(4)] protected GUIButtonControl btnScreenSetup = null;
    [SkinControl(5)] protected GUIButtonControl btnScreensaver = null;
    [SkinControl(6)] protected GUIButtonControl btnThumbnails = null;
    [SkinControl(7)] protected GUIButtonControl btnOnScreenDisplay = null;
    [SkinControl(8)] protected GUIButtonControl btnSkipSteps= null;
    [SkinControl(16)] protected GUICheckButton btnFileMenu= null;
    [SkinControl(18)] protected GUIButtonControl btnPin = null;
    
    private string _pin = string.Empty;


    public GUISettingsGUIMain()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUIMAIN; //1021
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        btnFileMenu.Selected = xmlreader.GetValueAsBool("filemenu", "enabled", true);
        btnPin.IsEnabled = btnFileMenu.Selected;
        _pin = Utils.DecryptPassword(xmlreader.GetValueAsString("filemenu", "pincode", ""));
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("filemenu", "enabled", btnFileMenu.Selected);
        xmlwriter.SetValue("filemenu", "pincode", Utils.EncryptPassword(_pin));
      }
    }

    #endregion

    #region Overrides

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnGeneral)
      {
        GUISettingsGUIGeneral guiSettingsGUIGeneral = (GUISettingsGUIGeneral)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUIGENERAL);
        if (guiSettingsGUIGeneral== null)
        {
          return;
        }

        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUIGENERAL);
      }
      if (control == btnSkin)
      {
        GUISettingsGUISkin guiSettingsGUISkin= (GUISettingsGUISkin)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUISKIN);
        if (guiSettingsGUISkin == null)
        {
          return;
        }

        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUISKIN);
      }
      if (control == btnScreenSetup)
      {
        GUISettingsGUIScreenSetup guiSettingsGUIScreenSetup = (GUISettingsGUIScreenSetup)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUISCREENSETUP);
        if (guiSettingsGUIScreenSetup == null)
        {
          return;
        }

        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUISCREENSETUP);
      }
      if (control == btnScreensaver)
      {
        GUISettingsGUIScreenSaver guiSettingsGUIScreenSaver = (GUISettingsGUIScreenSaver)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUISCREENSAVER);
        if (guiSettingsGUIScreenSaver == null)
        {
          return;
        }

        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUISCREENSAVER);
      }
      if (control == btnThumbnails)
      {
        GUISettingsGUIThumbnails guiSettingsThumbnails = (GUISettingsGUIThumbnails)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUITHUMBNAILS);
        if (guiSettingsThumbnails == null)
        {
          return;
        }
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUITHUMBNAILS);
      }
      if (control == btnSkipSteps)
      {
        GUISettingsGUISkipSteps guiSettingsSkipsteps = (GUISettingsGUISkipSteps)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUISKIPSTEPS);
        if (guiSettingsSkipsteps== null)
        {
          return;
        }
        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUISKIPSTEPS);
      }
      if (control == btnFileMenu)
      {
        if (btnFileMenu.Selected)
        {
          btnPin.IsEnabled = true;
        }
        else
        {
          btnPin.IsEnabled = false;
        }
        SettingsChanged(true);
      }
      if (control == btnPin)
      {
        if (_pin != string.Empty)
        {
          var dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          if (null == dlgOK)
          {
            return;
          }
          dlgOK.SetHeading("");
          dlgOK.SetLine(1, 100513);
          dlgOK.DoModal(GetID);

          if (!RequestPin())
          {
            return;
          }
        }

        var dlgOK2 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
        if (null == dlgOK2)
        {
          return;
        }
        dlgOK2.SetHeading("");
        dlgOK2.SetLine(1, 100514);
        dlgOK2.DoModal(GetID);

        SetPin();
        
        SettingsChanged(true);

      }
      if (control == btnOnScreenDisplay)
      {
        GUISettingsGUIOnScreenDisplay guiOnScreenDisplay = (GUISettingsGUIOnScreenDisplay)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GUIONSCREEN_DISPLAY);

        if (guiOnScreenDisplay == null)
        {
          return;
        }

        GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GUIONSCREEN_DISPLAY);
      }

      base.OnClicked(controlId, control, actionType);
    }

    private void SetPin()
    {
      var msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
      GUIWindowManager.SendMessage(msgGetPassword);
        
      _pin = msgGetPassword.Label;
    }

    private bool RequestPin()
    {
      bool retry = true;

      while (retry)
      {
        var msgGetPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
        GUIWindowManager.SendMessage(msgGetPassword);

        if (msgGetPassword.Label == _pin)
        {
          return true;
        }

        var msgWrongPassword = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WRONG_PASSWORD, 0, 0, 0, 0, 0,
                                                     0);
        GUIWindowManager.SendMessage(msgWrongPassword);

        if (!(bool)msgWrongPassword.Object)
        {
          retry = false;
        }
      }
      return false;
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101021)); //GUI
      LoadSettings();

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      SaveSettings();

      if (MediaPortal.GUI.Settings.GUISettings.SettingsChanged && !Utils.IsGUISettingsWindow(newWindowId))
      {
        MediaPortal.GUI.Settings.GUISettings.OnRestartMP(GetID);
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

    #endregion

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}