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
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsGeneral.
  /// </summary>
  public class GUISettingsGUIScreenSaver : GUIInternalWindow
  {
    [SkinControl(2)] protected GUICheckButton btnScreenSaverEnabled= null;
    [SkinControl(4)] protected GUICheckButton cmBlankScreen = null;
    [SkinControl(5)] protected GUICheckButton cmReduceFrameRate = null;

    private enum Controls
    {
      CONTROL_SCREENSAVER_DELAY = 3
    } ;
    
    private Int32 _screenSaverDelay = 300;
    private bool _settingsSaved;
    
    private class CultureComparer : IComparer
    {
      #region IComparer Members

      public int Compare(object x, object y)
      {
        CultureInfo info1 = (CultureInfo)x;
        CultureInfo info2 = (CultureInfo)y;
        return String.Compare(info1.EnglishName, info2.EnglishName, true);
      }

      #endregion
    }

    public GUISettingsGUIScreenSaver()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GUISCREENSAVER; //1020
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_GUI_ScreenSaver.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        btnScreenSaverEnabled.Selected = xmlreader.GetValueAsBool("general", "IdleTimer", true);
        EnableButtons(btnScreenSaverEnabled.Selected);


        _screenSaverDelay = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
        bool screenSaverType = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
        
        if (screenSaverType)
        {
          cmBlankScreen.Selected = true;
          cmReduceFrameRate.Selected = false;
        }
        else
        {
          cmBlankScreen.Selected = false;
          cmReduceFrameRate.Selected = true;
        }
      }
    }

    private void SaveSettings()
    {
      if (!_settingsSaved)
      {
        _settingsSaved = true;
        using (Settings xmlwriter = new MPSettings())
        {
          xmlwriter.SetValueAsBool("general", "IdleTimer", btnScreenSaverEnabled.Selected);
          xmlwriter.SetValue("general", "IdleTimeValue", _screenSaverDelay);
          xmlwriter.SetValueAsBool("general", "IdleBlanking", cmBlankScreen.Selected);
        }
      }
    }

    #endregion

    #region Overrides

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);

            for (int i = 1; i <= 10000; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_SCREENSAVER_DELAY, i.ToString());
            }
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_SCREENSAVER_DELAY, _screenSaverDelay - 1);
          }
          return true;

          case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_SCREENSAVER_DELAY)
            {
              string strLabel = message.Label;
              _screenSaverDelay = Int32.Parse(strLabel);
              GUIGraphicsContext.ScrollSpeedHorizontal = _screenSaverDelay;
              SettingsChanged(true);
            }
            break;
          }
      }
      return base.OnMessage(message);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      
      if (control == btnScreenSaverEnabled)
      {
        EnableButtons(btnScreenSaverEnabled.Selected);
        SettingsChanged(true);
      }
      if (control == cmBlankScreen)
      {
        cmReduceFrameRate.Selected = false;
        SettingsChanged(true);
      }
      if (control == cmReduceFrameRate)
      {
        cmBlankScreen.Selected = false;
        SettingsChanged(true);
      }
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnPageLoad()
    {
      _settingsSaved = false;
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101020));
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

    private void EnableButtons (bool enable)
    {
      if (enable)
      {
        cmBlankScreen.IsEnabled = true;
        cmReduceFrameRate.IsEnabled = true;
        GUIControl.EnableControl(GetID, (int)Controls.CONTROL_SCREENSAVER_DELAY);
      }
      else
      {
        cmBlankScreen.IsEnabled = false;
        cmReduceFrameRate.IsEnabled = false;
        GUIControl.DisableControl(GetID, (int)Controls.CONTROL_SCREENSAVER_DELAY);
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }

  }
}