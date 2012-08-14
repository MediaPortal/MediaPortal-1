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
using Action = MediaPortal.GUI.Library.Action;

namespace WindowPlugins.GUISettings
{
  /// <summary>
  /// Summary description for GUISettingsMovies.
  /// </summary>
  public class GUISettingsGeneralMain : GUIInternalWindow
  {
    [SkinControl(2)] protected GUIButtonControl btnGeneral = null;
    [SkinControl(3)] protected GUIButtonControl btnStartup = null;
    [SkinControl(4)] protected GUIButtonControl btnResume = null;
    [SkinControl(5)] protected GUIButtonControl btnVolume = null;
    [SkinControl(6)] protected GUIButtonControl btnRefreshRate = null;
    [SkinControl(7)] protected GUIButtonControl btnAutoplay = null;
    

    private int _selectedOption;

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

    public GUISettingsGeneralMain()
    {
      GetID = (int)Window.WINDOW_SETTINGS_GENERALMAIN;//1016
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_General.xml"));
    }

    #region Serialization

    private void LoadSettings()
    {
      
    }

    private void SaveSettings()
    {
      
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101016)); // General
      LoadSettings();

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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnGeneral)
      {
        OnGeneral();
      }
      if (control == btnStartup)
      {
        OnStartup();
      }
      if (control == btnResume)
      {
        OnResume();
      }
      if (control == btnVolume)
      {
        OnVolume();
      }
      if (control == btnRefreshRate)
      {
        OnRefreshRate();
      }
      if (control == btnAutoplay)
      {
        _selectedOption = -1;
        OnAutoPlay();
      }
      
      base.OnClicked(controlId, control, actionType);
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

    private void OnGeneral()
    {
      GUISettingsGeneralMP dlg = (GUISettingsGeneralMP)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GENERALMP);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GENERALMP);
    }

    private void OnStartup()
    {
      GUISettingsGeneralStartup dlg = (GUISettingsGeneralStartup)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GENERALSTARTUP);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GENERALSTARTUP);
    }

    private void OnResume()
    {
      GUISettingsGeneralResume dlg = (GUISettingsGeneralResume)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GENERALRESUME);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GENERALRESUME);
    }

    private void OnVolume()
    {
      GUISettingsGeneralVolume dlg = (GUISettingsGeneralVolume)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GENERALVOLUME);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GENERALVOLUME);
    }

    private void OnRefreshRate()
    {
      GUISettingsGeneralRefreshRate dlg = (GUISettingsGeneralRefreshRate)GUIWindowManager.GetWindow((int)Window.WINDOW_SETTINGS_GENERALREFRESHRATE);
      if (dlg == null)
      {
        return;
      }
      GUIWindowManager.ActivateWindow((int)Window.WINDOW_SETTINGS_GENERALREFRESHRATE);
    }

    private void OnAutoPlay()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(713)); //Autoplay

        dlg.AddLocalizedString(2135); // Audio
        dlg.AddLocalizedString(2134); // Video
        dlg.AddLocalizedString(300006); // Photo

        if (_selectedOption != -1)
          dlg.SelectedLabel = _selectedOption;

        dlg.DoModal(GetID);

        if (dlg.SelectedId == -1)
        {
          return;
        }

        _selectedOption = dlg.SelectedLabel;

        switch (dlg.SelectedId)
        {
          case 2135: // Audio
          case 2134: // Video
          case 300006: // Photo
            OnPlay(dlg.SelectedId);
            break;
        }
      }
    }

    private void OnPlay(int type)
    {
      string strHowToPlay = string.Empty;
      string strType = string.Empty;

      using (Settings xmlreader = new MPSettings())
      {
        if (type == 2135) // Audio
        {
          strType = "autoplay_video";
        }
        if (type == 2134) // Video
        {
          strType = "autoplay_audio";
        }
        if (type == 300006) // photo
        {
          strType = "autoplay_photo";
        }
        strHowToPlay = xmlreader.GetValueAsString("general", strType, "Ask");
      }
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(496)); //Options

      dlg.AddLocalizedString(208); // Play
      dlg.AddLocalizedString(300007); // Do not play
      dlg.AddLocalizedString(300008); // Ask what to do

      // Set options from config
      switch (strHowToPlay)
      {
        case "Yes":
          dlg.SelectedLabel = 0;
          break;
        case "No":
          dlg.SelectedLabel = 1;
          break;
        case "Ask":
          dlg.SelectedLabel = 2;
          break;
        default:
          dlg.SelectedLabel = 2;
          break;
      }
      // Show options
      dlg.DoModal(GetID);

      if (dlg.SelectedId == -1)
      {
        OnAutoPlay();
        return;
      }

      switch (dlg.SelectedId)
      {
        case 208: // Play
          strHowToPlay = "Yes";
          break;
        case 300007: // Do not play
          strHowToPlay = "No";
          break;
        case 300008: // Ask what to do
          strHowToPlay = "Ask";
          break;
      }

      using (Settings xmlwriter = new MPSettings())
      {

        xmlwriter.SetValue("general", strType, strHowToPlay);
      }
      
      OnAutoPlay();
    }

  }
}