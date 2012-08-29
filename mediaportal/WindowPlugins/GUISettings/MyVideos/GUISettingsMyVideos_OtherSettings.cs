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
using MediaPortal.GUI.Library;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Settings
{
  public class GUISettingsMoviesOther : GUIInternalWindow
  {
    // Grabbers
    [SkinControl(2)] protected GUICheckButton btnMarkWatched = null;
    [SkinControl(3)] protected GUICheckButton btnKeepFoldersTogether = null;
    [SkinControl(4)] protected GUICheckButton btnCommercialSkip = null;

    private int _watchedPercentage = 95;
    private int _videoAudioDelay = 50;
    
    private enum Controls
    {
      CONTROL_PLAYEDTIMEPERCENTAGE = 5,
      CONTROL_VIDEOAUDIODELAY = 6
    } ;
    
    public GUISettingsMoviesOther()
    {
      GetID = (int)Window.WINDOW_SETTINGS_VIDEOOTHERSETTINGS; //1023
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MyVideos_OtherSettings.xml"));
    }

    // Need change for 1.3.0
    #region Serialization

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Mark Watched
        btnMarkWatched.Selected = xmlreader.GetValueAsBool("movies", "markwatched", true);
        btnKeepFoldersTogether.Selected = xmlreader.GetValueAsBool("movies", "keepfolderstogether", false);
        btnCommercialSkip.Selected = xmlreader.GetValueAsBool("comskip", "automaticskip", false);
        _watchedPercentage = xmlreader.GetValueAsInt("movies", "playedpercentagewatched", 95);
        _videoAudioDelay = xmlreader.GetValueAsInt("FFDShow", "audiodelayInterval", 50);      
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValueAsBool("movies", "markwatched", btnMarkWatched.Selected);
        xmlwriter.SetValueAsBool("movies", "keepfolderstogether", btnKeepFoldersTogether.Selected);
        xmlwriter.SetValueAsBool("comskip", "automaticskip", btnCommercialSkip.Selected);

        xmlwriter.SetValue("movies", "playedpercentagewatched", _watchedPercentage);
        xmlwriter.SetValue("FFDShow", "audiodelayInterval", _videoAudioDelay);
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
            LoadSettings();
            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_PLAYEDTIMEPERCENTAGE);
            for (int i = 1; i <= 100; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_PLAYEDTIMEPERCENTAGE, i.ToString());
            }

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_VIDEOAUDIODELAY);
            for (int i = 1; i <= 100; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_VIDEOAUDIODELAY, i.ToString());
            }

            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_PLAYEDTIMEPERCENTAGE, _watchedPercentage - 1);
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIDEOAUDIODELAY, _videoAudioDelay - 1);

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_PLAYEDTIMEPERCENTAGE)
            {
              string strLabel = message.Label;
              _watchedPercentage = Int32.Parse(strLabel);
            }
            if (iControl == (int)Controls.CONTROL_VIDEOAUDIODELAY)
            {
              string strLabel = message.Label;
              if (Int32.Parse(strLabel) < 10)
              {
                GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_VIDEOAUDIODELAY, 9);
                _videoAudioDelay = 10;
                break;
              }
              _videoAudioDelay = Int32.Parse(strLabel);
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101023));
      LoadSettings();
      SetProperties();

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
    
    private void SetProperties()
    {
      
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
    }
  }
}
