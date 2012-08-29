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
  public class GUISettingsPicturesSlideshow : GUIInternalWindow
  {
    [SkinControl(5)] protected GUICheckButton cmXfade = null;
    [SkinControl(6)] protected GUICheckButton cmKenburns = null;
    [SkinControl(7)] protected GUICheckButton cmRandom = null;

    [SkinControl(8)] protected GUICheckButton cmLoopSlideShows = null;
    [SkinControl(9)] protected GUICheckButton cmShuffleSlideShows = null;
    [SkinControl(41)] protected GUICheckButton cmExifSlideShows = null;
    [SkinControl(42)] protected GUICheckButton cmPicasaSlideShows = null;
    [SkinControl(43)] protected GUICheckButton cmGroupByDaySlideShows = null;
    [SkinControl(44)] protected GUICheckButton cmEnablePlaySlideShows = null;
    [SkinControl(45)] protected GUICheckButton cmPlayInSlideShows = null;
    
    private enum Controls
    {
      CONTROL_SPEED = 2,
      CONTROL_TRANSITION = 3,
      CONTROL_KENBURNS_SPEED = 4
    } ;

    private int m_iSpeed = 3;
    private int m_iTransistion = 20;
    private int m_iKenBurnsSpeed = 30;
    private bool m_bXFade = false;
    private bool m_bKenBurns = false;
    private bool m_bRandom = false;
    
    public GUISettingsPicturesSlideshow()
    {
      GetID = (int)Window.WINDOW_SETTINGS_PICTURES_SLIDESHOW; //1015
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\settings_MyPictures_Slideshow.xml"));
    }

    #region Serialisation

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_iSpeed = xmlreader.GetValueAsInt("pictures", "speed", 3);
        m_iTransistion = xmlreader.GetValueAsInt("pictures", "transition", 20);
        m_iKenBurnsSpeed = xmlreader.GetValueAsInt("pictures", "kenburnsspeed", 20);
        m_bKenBurns = xmlreader.GetValueAsBool("pictures", "kenburns", false);
        m_bRandom = xmlreader.GetValueAsBool("pictures", "random", false);
        m_bXFade = (!m_bRandom & !m_bKenBurns);

        cmShuffleSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
        cmLoopSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "autoRepeat", false);

        cmExifSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "useExif", true);
        cmPicasaSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "usePicasa", false);
        cmGroupByDaySlideShows.Selected = xmlreader.GetValueAsBool("pictures", "useDayGrouping", false);
        cmEnablePlaySlideShows.Selected = xmlreader.GetValueAsBool("pictures", "enableVideoPlayback", false);
        cmPlayInSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "playVideosInSlideshows", false);
      }
    }

    private void SaveSettings()
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("pictures", "speed", m_iSpeed.ToString());
        xmlwriter.SetValue("pictures", "transition", m_iTransistion.ToString());
        xmlwriter.SetValue("pictures", "kenburnsspeed", m_iKenBurnsSpeed.ToString());
        xmlwriter.SetValueAsBool("pictures", "kenburns", m_bKenBurns);
        xmlwriter.SetValueAsBool("pictures", "random", m_bRandom);

        xmlwriter.SetValueAsBool("pictures", "autoRepeat", cmLoopSlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "autoShuffle", cmShuffleSlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "useExif", cmExifSlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "usePicasa", cmPicasaSlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "useDayGrouping", cmGroupByDaySlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "enableVideoPlayback", cmEnablePlaySlideShows.Selected);
        xmlwriter.SetValueAsBool("pictures", "playVideosInSlideshows", cmPlayInSlideShows.Selected);
      }
    }

    #endregion

    #region Overrides

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      GUIPropertyManager.SetProperty("#currentmodule", GUILocalizeStrings.Get(101015));

      if (!MediaPortal.Util.Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
      {
        if (MediaPortal.GUI.Settings.GUISettings.IsPinLocked() && !MediaPortal.GUI.Settings.GUISettings.RequestPin())
        {
          GUIWindowManager.CloseCurrentWindow();
        }
      }
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

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            LoadSettings();
            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_SPEED);
            for (int i = 1; i <= 10; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_SPEED, i.ToString());
            }

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_TRANSITION);
            for (int i = 1; i <= 50; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_TRANSITION, i.ToString());
            }

            GUIControl.ClearControl(GetID, (int)Controls.CONTROL_KENBURNS_SPEED);
            for (int i = 1; i <= 50; ++i)
            {
              GUIControl.AddItemLabelControl(GetID, (int)Controls.CONTROL_KENBURNS_SPEED, i.ToString());
            }

            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_SPEED, m_iSpeed - 1);
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_TRANSITION, m_iTransistion - 1);
            GUIControl.SelectItemControl(GetID, (int)Controls.CONTROL_KENBURNS_SPEED, m_iKenBurnsSpeed - 1);

            if (m_bXFade)
            {
              cmXfade.Selected = true;
            }
            if (m_bKenBurns)
            {
              cmKenburns.Selected = true;
            }
            if (m_bRandom)
            {
              cmRandom.Selected = true;
            }

            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          {
            SaveSettings();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId;
            if (iControl == (int)Controls.CONTROL_SPEED)
            {
              string strLabel = message.Label;
              m_iSpeed = Int32.Parse(strLabel);
            }
            if (iControl == (int)Controls.CONTROL_TRANSITION)
            {
              string strLabel = message.Label;
              m_iTransistion = Int32.Parse(strLabel);
            }
            if (iControl == (int)Controls.CONTROL_KENBURNS_SPEED)
            {
              string strLabel = message.Label;
              m_iKenBurnsSpeed = Int32.Parse(strLabel);
            }
            if (iControl == cmXfade.GetID)
            {
              m_bXFade = true;
              m_bKenBurns = false;
              m_bRandom = false;
              UpdateButtons();
              return true;
            }
            if (iControl == cmKenburns.GetID)
            {
              m_bXFade = false;
              m_bKenBurns = true;
              m_bRandom = false;
              UpdateButtons();
              return true;
            }
            if (iControl == cmRandom.GetID)
            {
              m_bXFade = false;
              m_bKenBurns = false;
              m_bRandom = true;
              UpdateButtons();
              return true;
            }
          }
          break;
      }
      return base.OnMessage(message);
    }
    
    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == cmLoopSlideShows || 
          control == cmShuffleSlideShows || 
          control == cmExifSlideShows ||
          control == cmPicasaSlideShows || 
          control == cmGroupByDaySlideShows || 
          control == cmEnablePlaySlideShows ||
          control == cmPlayInSlideShows || 
          control == cmXfade || 
          control == cmKenburns || 
          control == cmRandom)
      {
        SettingsChanged(true);
      }
      
      base.OnClicked(controlId, control, actionType);
    }

    #endregion

    private void UpdateButtons()
    {
      if (m_bRandom)
      {
        cmRandom.Selected = true;
      }
      else
      {
        cmRandom.Selected = false;
      }

      if (m_bXFade)
      {
        cmXfade.Selected = true;
      }
      else
      {
        cmXfade.Selected = false;
      }

      if (m_bKenBurns)
      {
        cmKenburns.Selected = true;
      }
      else
      {
        cmKenburns.Selected = false;
      }
    }

    private void SettingsChanged(bool settingsChanged)
    {
      MediaPortal.GUI.Settings.GUISettings.SettingsChanged = settingsChanged;
    }

  }
}