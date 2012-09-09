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
using WindowPlugins.GUISettings;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsPictures : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnSlideshow = null;
    [SkinControl(3)] protected GUIButtonControl btnExtensions = null;
    [SkinControl(4)] protected GUIButtonControl btnFolders = null;
    [SkinControl(5)] protected GUIButtonControl btnDatabase = null;

    private string section = "pictures";

    public GUISettingsPictures()
    {
      GetID = (int)Window.WINDOW_SETTINGS_PICTURES; //12
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MyPictures.xml"));
    }

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(100012));

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
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_HOME || action.wID == Action.ActionType.ACTION_SWITCH_HOME)
      {
        return;
      }

      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
          {
            GUIWindowManager.ShowPreviousWindow();
            return;
          }
      }
      base.OnAction(action);
    }
    
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control== btnSlideshow)
      {
        OnSlideShow();
      }
      if (control == btnFolders)
      {
        OnFolders();
      }
      if (control == btnExtensions)
      {
        OnExtensions();
      }
      if (control == btnDatabase)
      {
        OnDatabase();
      }
      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    private void OnSlideShow()
    {
      GUISettingsPicturesSlideshow dlg = (GUISettingsPicturesSlideshow)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_PICTURES_SLIDESHOW);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_PICTURES_SLIDESHOW);
    }

    private void OnFolders()
    {
      GUIShareFolders dlg = (GUIShareFolders)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_FOLDERS);
      if (dlg == null)
      {
        return;
      }
      dlg.Section = section;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_FOLDERS);
    }

    private void OnExtensions()
    {
      GUISettingsExtensions dlg = (GUISettingsExtensions)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_EXTENSIONS);
      if (dlg == null)
      {
        return;
      }
      dlg.Section = section;
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_EXTENSIONS);
    }

    private void OnDatabase()
    {
      GUISettingsPicturesDatabase dlg = (GUISettingsPicturesDatabase)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_PICTURESDATABASE);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_PICTURESDATABASE);
    }
    
  }
}