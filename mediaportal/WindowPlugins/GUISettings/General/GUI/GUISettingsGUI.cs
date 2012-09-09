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
        _pin = Utils.DecryptPin(xmlreader.GetValueAsString("filemenu", "pincode", ""));
      }
    }

    private void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("filemenu", "enabled", btnFileMenu.Selected);
        xmlwriter.SetValue("filemenu", "pincode", Utils.EncryptPin(_pin));
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
        string tmpPin = _pin;
        GetStringFromKeyboard(ref tmpPin, 4);

        int number;
        if (Int32.TryParse(tmpPin, out number))
        {
          _pin = number.ToString();
        }
        else
        {
          _pin = string.Empty;
        }
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
    
    private void GetStringFromKeyboard(ref string strLine, int maxLenght)
    {
      VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return;
      }
      keyboard.Reset();
      keyboard.Text = strLine;

      if (maxLenght > 0)
      {
        keyboard.SetMaxLength(maxLenght);
      }

      keyboard.DoModal(GUIWindowManager.ActiveWindow);

      if (keyboard.IsConfirmed)
      {
        strLine = keyboard.Text;
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }
  }
}