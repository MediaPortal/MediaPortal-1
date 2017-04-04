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
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Player.PostProcessing;
using MediaPortal.Player.Subtitles;
using MediaPortal.Playlists;
using MediaPortal.Video.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Video
{
  public class GUIVideoOSD : GUIInternalWindow
  {
    private enum Controls
    {
      OSD_VIDEOPROGRESS = 1,

      OSD_TIMEINFO = 100,
      Panel1 = 101,
      Panel2 = 150,

      OSD_PAUSE = 209,
      OSD_SKIPBWD = 210,
      OSD_REWIND = 211,
      OSD_STOP = 212,
      OSD_PLAY = 213,
      OSD_FFWD = 214,
      OSD_SKIPFWD = 215,

      OSD_MUTE = 216,
      // OSD_SYNC =217 - not used
      OSD_SUBTITLES = 218,
      OSD_BOOKMARKS = 219,
      OSD_VIDEO = 220,
      OSD_AUDIO = 221,

      OSD_SUBMENU_BG_VOL = 300,
      OSD_VOLUMESLIDER = 400,
      OSD_VOLUMESLIDER_LABEL = 450,

      // OSD_SUBMENU_BG_SYNC 301 - not used

      OSD_SUBMENU_BG_SUBTITLES = 302,
      OSD_SUBTITLE_DELAY = 800,
      OSD_SUBTITLE_DELAY_LABEL = 850,
      OSD_SUBTITLE_ONOFF = 801,
      OSD_SUBTITLE_LIST = 802,
      OSD_SUBTITLE_FORCED_ONOFF = 803,

      OSD_SUBMENU_BG_BOOKMARKS = 303,

      OSD_SUBMENU_BG_VIDEO = 304,
      OSD_BRIGHTNESS = 704,
      OSD_BRIGHTNESSLABEL = 752,
      OSD_CONTRAST = 705,
      OSD_CONTRASTLABEL = 753,
      OSD_GAMMA = 706,
      OSD_GAMMALABEL = 754,
      OSD_SHARPNESS = 716,
      OSD_SHARPNESSLABEL = 755,
      OSD_SATURATION = 717,
      OSD_SATURATIONLABEL = 756,
      OSD_NONINTERLEAVED = 701,
      OSD_NOCACHE = 702,
      OSD_ADJFRAMERATE = 703,
      OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF = 707,
      OSD_VIDEO_POSTPROC_RESIZE_ONOFF = 708,
      OSD_VIDEO_POSTPROC_CROP_ONOFF = 709,
      OSD_VIDEO_POSTPROC_CROP_VERTICAL = 711,
      OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL = 710,
      OSD_VIDEO_POSTPROC_CROP_HORIZONTAL = 713,
      OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL = 712,
      OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF = 714,

      OSD_SUBMENU_NIB = 350,
      OSD_AVDELAY = 500,
      OSD_AVDELAY_LABEL = 550,

      OSD_SUBMENU_BG_AUDIO = 305,
      OSD_AUDIOSTREAM_LIST = 501,
      OSD_AUDIOVOLUMESLIDER = 511,
      OSD_AUDIOVOLUMESLIDER_LABEL = 512,
      OSD_CREATEBOOKMARK = 600,
      OSD_BOOKMARKS_LIST = 601,
      OSD_CLEARBOOKMARKS = 602,
      OSD_BOOKMARKS_LIST_LABEL = 650,
      OSD_VIDEOPOS = 700,
      OSD_VIDEOPOS_LABEL = 750
    } ;

    private bool isSubMenuVisible = false;
    private int m_iActiveMenu = 0;
    private int m_iActiveMenuButtonID = 0;
    private int m_subtitleDelay = 0;
    private int m_delayInterval = 0;
    private int m_audioDelay = 0;
    private int m_delayIntervalAudio = 0;
    private bool m_bNeedRefresh = false;
    private int m_iCurrentBookmark = 0;
    private PlayListPlayer playlistPlayer;

    public GUIVideoOSD()
    {
      GetID = (int)Window.WINDOW_OSD;
      playlistPlayer = PlayListPlayer.SingletonPlayer;
    }

    public override bool Init()
    {
      bool bResult = Load(GUIGraphicsContext.GetThemedSkinFile(@"\videoOSD.xml"));
      GetID = (int)Window.WINDOW_OSD;
      return bResult;
    }

    public bool SubMenuVisible
    {
      get { return isSubMenuVisible; }
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void Render(float timePassed)
    {
      //SetVideoProgress();
      base.Render(timePassed); // render our controls to the screen
    }

    private void HideControl(int dwSenderId, int dwControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_HIDDEN, GetID, dwSenderId, dwControlID, 0, 0, null);
      OnMessage(msg);
    }

    private void ShowControl(int dwSenderId, int dwControlID)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID, dwSenderId, dwControlID, 0, 0, null);
      OnMessage(msg);
    }

    private void FocusControl(int dwSenderId, int dwControlID, int dwParam)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SETFOCUS, GetID, dwSenderId, dwControlID, dwParam,
                                      0, null);
      OnMessage(msg);
    }


    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_OSD_SHOW_LEFT:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_RIGHT:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_UP:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_DOWN:
          break;

        case Action.ActionType.ACTION_OSD_SHOW_SELECT:
          break;

        case Action.ActionType.ACTION_OSD_HIDESUBMENU:
          break;

        case Action.ActionType.ACTION_CONTEXT_MENU:
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        case Action.ActionType.ACTION_SHOW_OSD:
          {
            if (isSubMenuVisible) // is sub menu on?
            {
              FocusControl(GetID, m_iActiveMenuButtonID, 0); // set focus to last menu button
              ToggleSubMenu(0, m_iActiveMenu); // hide the currently active sub-menu
            }
            if (action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
            {
              GUIVideoFullscreen videoWindow = (GUIVideoFullscreen)GUIWindowManager.GetWindow((int)Window.WINDOW_FULLSCREEN_VIDEO);
              videoWindow.OnAction(new Action(Action.ActionType.ACTION_SHOW_OSD, 0, 0));
              videoWindow.OnAction(action);
            }
            return;
          }

        case Action.ActionType.ACTION_PAUSE:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_PAUSE,
                                               (int)Controls.OSD_PAUSE, 0, 0, null);
            OnMessage(msgSet);

            if (g_Player.Paused)
            {
              GUIWindowManager.IsPauseOsdVisible = true;
            }
            else
            {
              GUIWindowManager.IsPauseOsdVisible = false;
            }
            return;
          }
 
        case Action.ActionType.ACTION_PLAY:
        case Action.ActionType.ACTION_MUSIC_PLAY:
          {
            g_Player.Speed = 1; // Single speed
            ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
            ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
            ToggleButton((int)Controls.OSD_PLAY, false); // their up state
            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_STOP:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_STOP,
                                               (int)Controls.OSD_STOP, 0, 0, null);
            OnMessage(msgSet);

            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_FORWARD:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_FFWD,
                                               (int)Controls.OSD_FFWD, 0, 0, null);
            OnMessage(msgSet);

            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_REWIND:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_REWIND,
                                               (int)Controls.OSD_REWIND, 0, 0, null);
            OnMessage(msgSet);

            GUIWindowManager.IsPauseOsdVisible = false;
            return;
          }

        case Action.ActionType.ACTION_OSD_SHOW_VALUE_PLUS:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_SKIPFWD,
                                               (int)Controls.OSD_SKIPFWD, 0, 0, null);
            OnMessage(msgSet);
            return;
          }

        case Action.ActionType.ACTION_OSD_SHOW_VALUE_MIN:
          {
            // push a message through to this window to handle the remote control button
            GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, GetID, (int)Controls.OSD_SKIPBWD,
                                               (int)Controls.OSD_SKIPBWD, 0, 0, null);
            OnMessage(msgSet);
            return;
          }
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT: // fired when OSD is hidden
          {
            //if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(true);
            // following line should stay. Problems with OSD not
            // appearing are already fixed elsewhere
            //for (int i = (int)Controls.Panel1; i < (int)Controls.Panel2; i++)
            //{
            //  HideControl(GetID, i);
            //}
            Dispose();
            GUIPropertyManager.SetProperty("#currentmodule", GUIWindowManager.GetWindow(message.Param1).GetModuleName());
            return true;
          }
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT: // fired when OSD is shown
          {
            // following line should stay. Problems with OSD not
            // appearing are already fixed elsewhere
            GUIPropertyManager.SetProperty("#currentmodule", GetModuleName());

            AllocResources();
            // if (g_application.m_pPlayer) g_application.m_pPlayer.ShowOSD(false);
            ResetAllControls(); // make sure the controls are positioned relevant to the OSD Y offset
            isSubMenuVisible = false;
            m_iActiveMenuButtonID = 0;
            m_iActiveMenu = 0;
            m_bNeedRefresh = false;
            Reset();
            FocusControl(GetID, (int)Controls.OSD_PLAY, 0); // set focus to play button by default when window is shown
            QueueAnimation(AnimationType.WindowOpen);
            for (int i = (int)Controls.Panel1; i < (int)Controls.Panel2; i++)
            {
              ShowControl(GetID, i);
            }
            if (g_Player.Paused)
            {
              ToggleButton((int)Controls.OSD_PLAY, true);
              // make sure play button is down (so it shows the pause symbol)
            }
            else
            {
              ToggleButton((int)Controls.OSD_PLAY, false); // make sure play button is up (so it shows the play symbol)
            }
            m_delayInterval = MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayInterval;
            if (m_delayInterval > 0)
              m_subtitleDelay = MediaPortal.Player.Subtitles.SubEngine.GetInstance().Delay / m_delayInterval;
            m_delayIntervalAudio = PostProcessingEngine.GetInstance().AudioDelayInterval;
            if (m_delayIntervalAudio > 0)
              m_audioDelay = PostProcessingEngine.GetInstance().AudioDelay / m_delayIntervalAudio;

            g_Player.UpdateMediaInfoProperties();
            GUIPropertyManager.SetProperty("#TV.View.HasTeletext", "False");
            return true;
          }

        case GUIMessage.MessageType.GUI_MSG_SETFOCUS:
          goto case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS;

        case GUIMessage.MessageType.GUI_MSG_LOSTFOCUS:
          {
            if (message.SenderControlId == 13)
            {
              return true;
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            int iControl = message.SenderControlId; // get the ID of the control sending us a message

            if (iControl >= (int)Controls.OSD_VOLUMESLIDER)
              // one of the settings (sub menu) controls is sending us a message
            {
              Handle_ControlSetting(iControl, message.Param1);
            }

            if (iControl == (int)Controls.OSD_PAUSE)
            {
              if (g_Player.Paused)
              {
                ToggleButton((int)Controls.OSD_PLAY, true);
                // make sure play button is down (so it shows the pause symbol)                
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_PLAY, false);
                // make sure play button is up (so it shows the play symbol)
                if (g_Player.Speed < 1) // are we not playing back at normal speed
                {
                  ToggleButton((int)Controls.OSD_REWIND, true); // make sure out button is in the down position
                  ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                }
                else
                {
                  ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                  if (g_Player.Speed == 1)
                  {
                    ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                  }
                }
              }
            }

            if (iControl == (int)Controls.OSD_PLAY)
            {
              //TODO
              int iSpeed = g_Player.Speed;
              if (iSpeed != 1) // we're in ffwd or rewind mode
              {
                g_Player.Speed = 1; // drop back to single speed
                ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
                ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
                ToggleButton((int)Controls.OSD_PLAY, false); // their up state
              }
              else
              {
                g_Player.Pause(); // Pause/Un-Pause playback
                if (g_Player.Paused)
                {
                  ToggleButton((int)Controls.OSD_PLAY, true);
                  // make sure play button is down (so it shows the pause symbol)
                }
                else
                {
                  ToggleButton((int)Controls.OSD_PLAY, false);
                  // make sure play button is up (so it shows the play symbol)
                }
              }
            }

            if (iControl == (int)Controls.OSD_STOP)
            {
              if (isSubMenuVisible) // sub menu currently active ?
              {
                FocusControl(GetID, m_iActiveMenuButtonID, 0); // set focus to last menu button
                ToggleSubMenu(0, m_iActiveMenu); // hide the currently active sub-menu
              }
              //g_application.m_guiWindowFullScreen.m_bOSDVisible = false;	// toggle the OSD off so parent window can de-init
              Log.Info("GUIVideoOSD:stop");
              g_Player.Stop(); // close our media
              //GUIWindowManager.ShowPreviousWindow();							// go back to the previous window
            }

            if (iControl == (int)Controls.OSD_REWIND)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause(); // Unpause playback
              }

              if (g_Player.Speed < 1) // are we not playing back at normal speed
              {
                ToggleButton((int)Controls.OSD_REWIND, true); // make sure out button is in the down position
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                if (g_Player.Speed == 1)
                {
                  ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                }
              }
            }

            if (iControl == (int)Controls.OSD_FFWD)
            {
              if (g_Player.Paused)
              {
                g_Player.Pause(); // Unpause playback
              }

              if (g_Player.Speed > 1) // are we not playing back at normal speed
              {
                ToggleButton((int)Controls.OSD_FFWD, true); // make sure out button is in the down position
                ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
              }
              else
              {
                ToggleButton((int)Controls.OSD_FFWD, false); // pop the button back to it's up state
                if (g_Player.Speed == 1)
                {
                  ToggleButton((int)Controls.OSD_REWIND, false); // pop the button back to it's up state
                }
              }
            }

            if (iControl == (int)Controls.OSD_SKIPBWD)
            {
              if (g_Player.IsDVD && g_Player.Playing)
              {
                Action actionPrevChapter = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
                GUIGraphicsContext.OnAction(actionPrevChapter);
              }
              else
              {
                playlistPlayer.PlayPrevious();
              }
              ToggleButton((int)Controls.OSD_SKIPBWD, false); // pop the button back to it's up state
              return true;
            }

            if (iControl == (int)Controls.OSD_SKIPFWD)
            {
              if (g_Player.IsDVD && g_Player.Playing)
              {
                Action actionPrevChapter = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
                GUIGraphicsContext.OnAction(actionPrevChapter);
              }
              else
              {
                playlistPlayer.PlayNext();
              }
              ToggleButton((int)Controls.OSD_SKIPFWD, false); // pop the button back to it's up state
              return true;
            }

            if (iControl == (int)Controls.OSD_MUTE)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VOL); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                int iValue = g_Player.Volume;
                GUISliderControl pSlider = GetControl((int)Controls.OSD_VOLUMESLIDER) as GUISliderControl;
                if (null != pSlider)
                {
                  pSlider.Percentage = iValue; // Update our volume slider accordingly ...
                }
                ShowControl(GetID, (int)Controls.OSD_VOLUMESLIDER); // show the volume control
                ShowControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
                FocusControl(GetID, (int)Controls.OSD_VOLUMESLIDER, 0); // set focus to it
              }
              else // sub menu is off
              {
                FocusControl(GetID, (int)Controls.OSD_MUTE, 0); // set focus to the mute button
              }
            }

            if (iControl == (int)Controls.OSD_SUBTITLES)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_SUBTITLES); // hide or show the sub-menu
              if (isSubMenuVisible)
              {
                // set the controls values
                GUISliderControl pControl = (GUISliderControl)GetControl((int)Controls.OSD_SUBTITLE_DELAY);
                pControl.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
                pControl.FloatInterval = 1;
                pControl.SetRange(-50, 50);
                SetSliderValue(-50, 50, m_subtitleDelay, (int)Controls.OSD_SUBTITLE_DELAY);
                SetCheckmarkValue(g_Player.EnableSubtitle, (int)Controls.OSD_SUBTITLE_ONOFF);
                SetCheckmarkValue(!g_Player.EnableForcedSubtitle, (int)Controls.OSD_SUBTITLE_FORCED_ONOFF);
                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_FORCED_ONOFF);
                ShowControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

                FocusControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY, 0);
                // set focus to the first control in our group
                PopulateSubTitles(); // populate the list control with subtitles for this video
              }
            }

            if (iControl == (int)Controls.OSD_BOOKMARKS)
            {
              GUIVideoFullscreen videoWindow = (GUIVideoFullscreen)GUIWindowManager.GetWindow((int)Window.WINDOW_FULLSCREEN_VIDEO);
              videoWindow.ShowBookmarksMenu();
              ToggleButton((int)Controls.OSD_BOOKMARKS, false);
            }

            if (iControl == (int)Controls.OSD_VIDEO)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_VIDEO); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                // set the controls values
                float fPercent = (float)(100 * (g_Player.CurrentPosition / g_Player.Duration));
                SetSliderValue(0.0f, 100.0f, (float)fPercent, (int)Controls.OSD_VIDEOPOS);

                bool hasPostProc = g_Player.HasPostprocessing;
                if (hasPostProc)
                {
                  IPostProcessingEngine engine = PostProcessingEngine.GetInstance();
                  SetCheckmarkValue(engine.EnablePostProcess, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
                  SetCheckmarkValue(engine.EnableResize, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
                  SetCheckmarkValue(engine.EnableCrop, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
                  SetCheckmarkValue(engine.EnableDeinterlace, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
                  UpdatePostProcessing();
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
                  ShowControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);
                }

                //SetCheckmarkValue(g_stSettings.m_bNonInterleaved, Controls.OSD_NONINTERLEAVED);
                //SetCheckmarkValue(g_stSettings.m_bNoCache, Controls.OSD_NOCACHE);
                //SetCheckmarkValue(g_stSettings.m_bFrameRateConversions, Controls.OSD_ADJFRAMERATE);

                UpdateGammaContrastBrightness();
                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_VIDEOPOS);
                ShowControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
                ShowControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
                ShowControl(GetID, (int)Controls.OSD_NOCACHE);
                ShowControl(GetID, (int)Controls.OSD_ADJFRAMERATE);
                ShowControl(GetID, (int)Controls.OSD_SATURATIONLABEL);
                ShowControl(GetID, (int)Controls.OSD_SATURATION);
                ShowControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);
                ShowControl(GetID, (int)Controls.OSD_SHARPNESS);
                ShowControl(GetID, (int)Controls.OSD_BRIGHTNESS);
                ShowControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);
                ShowControl(GetID, (int)Controls.OSD_CONTRAST);
                ShowControl(GetID, (int)Controls.OSD_CONTRASTLABEL);
                ShowControl(GetID, (int)Controls.OSD_GAMMA);
                ShowControl(GetID, (int)Controls.OSD_GAMMALABEL);
                FocusControl(GetID, (int)Controls.OSD_VIDEOPOS, 0); // set focus to the first control in our group
              }
            }

            if (iControl == (int)Controls.OSD_AUDIO)
            {
              ToggleSubMenu(iControl, (int)Controls.OSD_SUBMENU_BG_AUDIO); // hide or show the sub-menu
              if (isSubMenuVisible) // is sub menu on?
              {
                int iValue = g_Player.Volume;
                GUISliderControl pSlider = GetControl((int)Controls.OSD_AUDIOVOLUMESLIDER) as GUISliderControl;
                if (null != pSlider)
                {
                  pSlider.Percentage = iValue; // Update our volume slider accordingly ...
                }

                // set the controls values
                GUISliderControl pControl = (GUISliderControl)GetControl((int)Controls.OSD_AVDELAY);
                pControl.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
                pControl.SetRange(-20, 20);
                SetSliderValue(-20, 20, m_audioDelay, (int)Controls.OSD_AVDELAY);

                bool hasPostProc = g_Player.HasPostprocessing;
                if (hasPostProc)
                {
                  GUIPropertyManager.SetProperty("#VideoOSD.AudioVideoDelayPossible", "true");
                  pControl.FloatInterval = 1;
                }
                else
                { 
                  GUIPropertyManager.SetProperty("#VideoOSD.AudioVideoDelayPossible", "false");
                  pControl.FloatValue = 0;
                  m_audioDelay = 0;
                  pControl.FloatInterval = 0;
                }

                // show the controls on this sub menu
                ShowControl(GetID, (int)Controls.OSD_AVDELAY);
                ShowControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);
                ShowControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
                ShowControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
                ShowControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

                FocusControl(GetID, (int)Controls.OSD_AVDELAY, 0); // set focus to the first control in our group
                PopulateAudioStreams(); // populate the list control with audio streams for this video
              }
            }

            return true;
          }
      }
      return base.OnMessage(message);
    }

    private void UpdateGammaContrastBrightness()
    {
      float fBrightNess = (float)GUIGraphicsContext.Brightness;
      float fContrast = (float)GUIGraphicsContext.Contrast;
      float fGamma = (float)GUIGraphicsContext.Gamma;
      float fSaturation = (float)GUIGraphicsContext.Saturation;
      float fSharpness = (float)GUIGraphicsContext.Sharpness;

      SetSliderValue(0.0f, 100.0f, (float)fBrightNess, (int)Controls.OSD_BRIGHTNESS);
      SetSliderValue(0.0f, 100.0f, (float)fContrast, (int)Controls.OSD_CONTRAST);
      SetSliderValue(0.0f, 100.0f, (float)fGamma, (int)Controls.OSD_GAMMA);
      SetSliderValue(0.0f, 100.0f, (float)fSaturation, (int)Controls.OSD_SATURATION);
      SetSliderValue(0.0f, 100.0f, (float)fSharpness, (int)Controls.OSD_SHARPNESS);
    }

    private void UpdatePostProcessing()
    {
      IPostProcessingEngine engine = PostProcessingEngine.GetInstance();
      SetSliderValue(0.0f, 100.0f, (float)engine.CropVertical, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      SetSliderValue(0.0f, 100.0f, (float)engine.CropHorizontal, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
    }

    private void ToggleButton(int iButtonID, bool bSelected)
    {
      GUIControl pControl = GetControl(iButtonID) as GUIControl;

      if (pControl != null)
      {
        if (bSelected) // do we want the button to appear down?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
        else // or appear up?
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID, 0, iButtonID, 0, 0, null);
          OnMessage(msg);
        }
      }
    }

    private void ToggleSubMenu(int iButtonID, int iBackID)
    {
      int iX, iY;

      GUIImage pImgNib = GetControl((int)Controls.OSD_SUBMENU_NIB) as GUIImage; // pointer to the nib graphic
      GUIImage pImgBG = GetControl(iBackID) as GUIImage; // pointer to the background graphic
      GUIControl pButton = GetControl(iButtonID);
      // pointer to the OSD menu button

      // check to see if we are currently showing a sub-menu and it's position is different
      if (isSubMenuVisible && iBackID != m_iActiveMenu)
      {
        m_bNeedRefresh = true;
        isSubMenuVisible = false; // toggle it ready for the new menu requested
      }

      // Get button position
      if (pButton != null)
      {
        iX = (pButton.XPosition + (pButton.Width / 2)); // center of button
        iY = pButton.YPosition;
      }
      else
      {
        iX = 0;
        iY = 0;
      }

      // Set nib position
      if (pImgNib != null && pImgBG != null)
      {
        pImgNib.SetPosition(iX - (pImgNib.TextureWidth / 2), iY - pImgNib.TextureHeight);

        if (!isSubMenuVisible) // sub menu not currently showing?
        {
          pImgNib.Visible = true; // make it show
          pImgBG.Visible = true; // make it show
        }
        else
        {
          pImgNib.Visible = false; // hide it
          pImgBG.Visible = false; // hide it
        }
      }

      isSubMenuVisible = !isSubMenuVisible; // toggle sub menu visible status
      if (!isSubMenuVisible)
      {
        m_bNeedRefresh = true;
      }

      GUIPropertyManager.SetProperty("#VideoOSD.AudioVideoDelayPossible", " ");

      // Set all sub menu controls to hidden
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

      HideControl(GetID, (int)Controls.OSD_AVDELAY);
      HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);

      HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
      HideControl(GetID, (int)Controls.OSD_NOCACHE);
      HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);

      HideControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);
      HideControl(GetID, (int)Controls.OSD_SHARPNESS);

      HideControl(GetID, (int)Controls.OSD_SATURATIONLABEL);
      HideControl(GetID, (int)Controls.OSD_SATURATION);

      HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
      HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_GAMMA);
      HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);

      HideControl(GetID, (int)Controls.OSD_CONTRAST);
      HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

      HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
      HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_FORCED_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);

      // Reset the other buttons back to up except the one that's active
      if (iButtonID != (int)Controls.OSD_MUTE)
      {
        ToggleButton((int)Controls.OSD_MUTE, false);
      }
      //if (iButtonID != (int)Controls.OSD_SYNC) ToggleButton((int)Controls.OSD_SYNC, false); - not used
      if (iButtonID != (int)Controls.OSD_SUBTITLES)
      {
        ToggleButton((int)Controls.OSD_SUBTITLES, false);
      }
      if (iButtonID != (int)Controls.OSD_BOOKMARKS)
      {
        ToggleButton((int)Controls.OSD_BOOKMARKS, false);
      }
      if (iButtonID != (int)Controls.OSD_VIDEO)
      {
        ToggleButton((int)Controls.OSD_VIDEO, false);
      }
      if (iButtonID != (int)Controls.OSD_AUDIO)
      {
        ToggleButton((int)Controls.OSD_AUDIO, false);
      }

      if (m_iActiveMenu > 1 && m_iActiveMenu != iBackID)
        GUIControl.HideControl(GetID, m_iActiveMenu);

      m_iActiveMenu = iBackID;
      m_iActiveMenuButtonID = iButtonID;
    }

    private float GetSliderValue(int iControlID)
    {
      GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);

      if (null != pControl)
      {
        switch (pControl.SpinType)
        {
          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
            return pControl.FloatValue;

          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
            return pControl.IntValue;

          default:
            return pControl.Percentage;
        }
      }

      return 0.0f;
    }

    private void SetSliderValue(float fMin, float fMax, float fValue, int iControlID)
    {
      GUISliderControl pControl = GetControl(iControlID) as GUISliderControl;

      if (null != pControl)
      {
        switch (pControl.SpinType)
        {
          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT:
            pControl.SetFloatRange(fMin, fMax);
            pControl.FloatValue = fValue;
            break;

          case GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT:
            pControl.SetRange((int)fMin, (int)fMax);
            pControl.IntValue = (int)fValue;
            break;

          default:
            pControl.Percentage = (int)fValue;
            break;
        }
      }
    }

    private void SetCheckmarkValue(bool bValue, int iControlID)
    {
      if (bValue)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SELECTED, GetID, 0, iControlID, 0, 0, null);
        OnMessage(msg);
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DESELECTED, GetID, 0, iControlID, 0, 0, null);
        OnMessage(msg);
      }
    }

    private void Handle_ControlSetting(int iControlID, long wID)
    {
      string strMovie = g_Player.CurrentFile;

      switch (iControlID)
      {
        case (int)Controls.OSD_VOLUMESLIDER:
        case (int)Controls.OSD_AUDIOVOLUMESLIDER:
          {
            GUISliderControl pControl = GetControl(iControlID) as GUISliderControl;
            if (null != pControl)
            {
              // no volume control yet so no code here at the moment
              if (g_Player.Playing)
              {
                int iPercentage = pControl.Percentage;
                g_Player.Volume = iPercentage;
              }
            }
          }
          break;

        case (int)Controls.OSD_VIDEOPOS:
          {
            g_Player.SeekAsolutePercentage((int)GetSliderValue(iControlID));
          }
          break;

        case (int)Controls.OSD_SATURATION:
          {
            GUIGraphicsContext.Saturation = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_SHARPNESS:
          {
            GUIGraphicsContext.Sharpness = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_BRIGHTNESS:
          {
            GUIGraphicsContext.Brightness = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_CONTRAST:
          {
            GUIGraphicsContext.Contrast = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_GAMMA:
          {
            GUIGraphicsContext.Gamma = (int)GetSliderValue(iControlID);
            UpdateGammaContrastBrightness();
          }
          break;

        case (int)Controls.OSD_AUDIOSTREAM_LIST:
          {
            if (wID != 0) // check to see if list control has an action ID, remote can cause 0 based events
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                              (int)Controls.OSD_AUDIOSTREAM_LIST, 0, 0, null);
              OnMessage(msg);
              // only change the audio stream if a different one has been asked for
              if (g_Player.CurrentAudioStream != msg.Param1)
              {
                g_Player.CurrentAudioStream = msg.Param1; // Set the audio stream to the one selected
                //ToggleSubMenu(0, m_iActiveMenu);						// hide the currently active sub-menu
                PopulateAudioStreams();
              }
            }
          }
          break;

        case (int)Controls.OSD_CREATEBOOKMARK:
          {
            double dCurTime = g_Player.CurrentPosition; // get the current playing time position

            VideoDatabase.AddBookMarkToMovie(g_Player.CurrentFile, (float)dCurTime); // add the current timestamp
            PopulateBookmarks(); // refresh our list control
          }
          break;

        case (int)Controls.OSD_BOOKMARKS_LIST:
          {
            if (wID != 0) // check to see if list control has an action ID, remote can cause 0 based events
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                              (int)Controls.OSD_BOOKMARKS_LIST, 0, 0, null);
              OnMessage(msg);
              m_iCurrentBookmark = msg.Param1; // index of bookmark user selected


              ArrayList bookmarks = new ArrayList();
              VideoDatabase.GetBookMarksForMovie(strMovie, ref bookmarks); // load the stored bookmarks
              if (bookmarks.Count <= 0)
              {
                return; // no bookmarks? leave if so ...
              }
              Log.Debug("GUIVideoOSD.BOOKMARKS_LIST - skipping");
              g_Player.SeekAbsolute((double)bookmarks[m_iCurrentBookmark]); // set mplayers play position
            }
          }
          break;

        case (int)Controls.OSD_CLEARBOOKMARKS:
          {
            VideoDatabase.ClearBookMarksOfMovie(g_Player.CurrentFile); // empty the bookmarks table for this movie
            m_iCurrentBookmark = 0; // reset current bookmark
            PopulateBookmarks(); // refresh our list control
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnablePostProcess = !PostProcessingEngine.GetInstance().EnablePostProcess;
          }
          break;

       case (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnableResize = !PostProcessingEngine.GetInstance().EnableResize;
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF:
          {
            PostProcessingEngine.GetInstance().EnableDeinterlace = !PostProcessingEngine.GetInstance().EnableDeinterlace;
          }
          break;

        case (int)Controls.OSD_SUBTITLE_ONOFF:
          {
            g_Player.EnableSubtitle = !g_Player.EnableSubtitle;
          }
          break;

        case (int)Controls.OSD_SUBTITLE_FORCED_ONOFF:
          {
            SubEngine.engine.AutoShow = !SubEngine.engine.AutoShow;
          }
          break;

        case (int)Controls.OSD_SUBTITLE_LIST:
          {
            if (wID != 0) // check to see if list control has an action ID, remote can cause 0 based events
            {
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED, GetID, 0,
                                              (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
              OnMessage(msg); // retrieve the selected list item
              if (g_Player.SupportsCC) // Subtitle CC
              {
                if (g_Player.SupportsCC && msg.Param1 == 0)
                {
                  msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_SELECTED_ITEM, GetID, 0,
                                       (int)Controls.OSD_SUBTITLE_LIST, msg.Param1, 0, null);
                  g_Player.EnableSubtitle = false;
                  g_Player.CurrentSubtitleStream = -1;
                  Log.Info("Subtitle CC selected ");
                }
                else
                {
                  Log.Info("Subtitle stream selected " + msg.Label);
                  g_Player.CurrentSubtitleStream = msg.Param1 - 1; // set the current subtitle
                  g_Player.EnableSubtitle = true;
                }
              }
              else
              {
                Log.Info("Subtitle stream selected " + msg.Label);
                g_Player.CurrentSubtitleStream = msg.Param1; // set the current subtitle
              }
              PopulateSubTitles();
            }
          }
          break;

        case (int)Controls.OSD_SUBTITLE_DELAY:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl && g_Player.EnableSubtitle)
            {
              if (pControl.FloatValue < m_subtitleDelay)
              {
                MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayMinus((int)pControl.FloatValue);
              }
              else if (pControl.FloatValue > m_subtitleDelay)
              {
                MediaPortal.Player.Subtitles.SubEngine.GetInstance().DelayPlus((int)pControl.FloatValue);
              }
              m_subtitleDelay = (int)pControl.FloatValue;
            }
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl)
            {
              PostProcessingEngine.GetInstance().CropVertical = pControl.Percentage;
              UpdatePostProcessing();
            }
          }
          break;

        case (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            if (null != pControl)
            {
              PostProcessingEngine.GetInstance().CropHorizontal = pControl.Percentage;
              UpdatePostProcessing();
            }
          }
          break;

        case (int)Controls.OSD_AVDELAY:
          {
            GUISliderControl pControl = (GUISliderControl)GetControl(iControlID);
            IPostProcessingEngine engine = PostProcessingEngine.GetInstance();

            if (null != pControl && g_Player.HasPostprocessing)
            {
              if (pControl.FloatValue < m_audioDelay)
              { 
                  PostProcessingEngine.GetInstance().AudioDelayMinus();
              }
              else if (pControl.FloatValue > m_audioDelay)
              { 
                  PostProcessingEngine.GetInstance().AudioDelayPlus();
              }
              m_audioDelay = (int)pControl.FloatValue;
            }
          }
          break;
      }
    }

    private void PopulateBookmarks()
    {
      ArrayList bookmarks = new ArrayList();
      string strMovie = g_Player.CurrentFile;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl = (GUIListControl)GetControl((int)Controls.OSD_BOOKMARKS_LIST);
      if (null == pControl)
      {
        pControl.SetPageControlVisible(false);
      }

      VideoDatabase.AddMovieFile(strMovie);
      // open the d/b and retrieve the bookmarks for the current movie
      VideoDatabase.GetBookMarksForMovie(strMovie, ref bookmarks);

      // empty the list ready for population
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0,
                                      (int)Controls.OSD_BOOKMARKS_LIST, 0, 0, null);
      OnMessage(msg);

      // cycle through each stored bookmark and add it to our list control
      for (int i = 0; i < (int)(bookmarks.Count); ++i)
      {
        string strItem;
        double fTime = (double)bookmarks[i];
        long lPTS1 = (long)(fTime);
        int hh = (int)(lPTS1 / 3600) % 100;
        int mm = (int)((lPTS1 / 60) % 60);
        int ss = (int)((lPTS1 / 1) % 60);
        strItem = String.Format("{0:00}.   {1:00}:{2:00}:{3:00}", i + 1, hh, mm, ss);

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label = strItem;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_BOOKMARKS_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }

      // set the currently active bookmark as the selected item in the list control
      GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                         (int)Controls.OSD_BOOKMARKS_LIST, m_iCurrentBookmark, 0, null);
      OnMessage(msgSet);
    }

    private void PopulateAudioStreams()
    {
      // tell the list control not to show the page x/y spin control
      GUIListControl pControl = (GUIListControl)GetControl((int)Controls.OSD_AUDIOSTREAM_LIST);
      if (null != pControl)
      {
        pControl.SetPageControlVisible(false);
      }

      // empty the list ready for population
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0,
                                      (int)Controls.OSD_AUDIOSTREAM_LIST, 0, 0, null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(460); // "Audio Stream"
      string strActiveLabel = GUILocalizeStrings.Get(461); // "[active]"

      // get the number of audio strams for the current movie
      int iValue = g_Player.AudioStreams;
      int currentAudioStream = g_Player.CurrentAudioStream;

      // cycle through each audio stream and add it to our list control
      for (int i = 0; i < iValue; ++i)
      {
        string strItem;
        string strLang = g_Player.AudioLanguage(i);
        string strName = g_Player.AudioType(i);
        // formats right label2 to '[active]'
        string strActive = (currentAudioStream == i) ? strActiveLabel : null;
        int ipos = strLang.IndexOf("[");
        if (ipos > 0)
        {
          strLang = strLang.Substring(0, ipos);
        }
        // formats to 'Language'
        strItem = String.Format(strLang);

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label = strItem;

        if (strActive != null) pItem.Label = strItem + " " + strActiveLabel;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_AUDIOSTREAM_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }

      // set the current active audio stream as the selected item in the list control
      GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                         (int)Controls.OSD_AUDIOSTREAM_LIST, g_Player.CurrentAudioStream, 0, null);
      OnMessage(msgSet);
    }

    private void PopulateSubTitles()
    {
      // get the number of subtitles in the current movie
      int subStreamsCount = g_Player.SubtitleStreams;

      // tell the list control not to show the page x/y spin control
      GUIListControl pControl = GetControl((int)Controls.OSD_SUBTITLE_LIST) as GUIListControl;
      if (null != pControl)
      {
        pControl.SetPageControlVisible(false);
      }

      // empty the list ready for population
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_RESET, GetID, 0,
                                      (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
      OnMessage(msg);

      string strLabel = GUILocalizeStrings.Get(462); // "Subtitle"
      string strActiveLabel = GUILocalizeStrings.Get(461); // "[active]"

      // cycle through each subtitle and add it to our list control
      int currentSubtitleStream = g_Player.CurrentSubtitleStream;
      if (g_Player.SupportsCC) // The current subtitle CC is not in the list, add it on top of it
      {
        string strActive = (g_Player.SupportsCC) ? strActiveLabel : null;
        string CC1 = "CC1";

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem();
        pItem.Label = CC1;

        if (currentSubtitleStream == -1)
          if (strActive != null) pItem.Label = "CC1" + " " + strActiveLabel;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_SUBTITLE_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }
      for (int i = 0; i < subStreamsCount; ++i)
      {
        string strItem;
        string strLang = g_Player.SubtitleLanguage(i);
        // formats right label2 to '[active]'
        string strActive = (currentSubtitleStream == i) ? strActiveLabel : null;
        int ipos = strLang.IndexOf("[");
        if (ipos > 0)
        {
          strLang = strLang.Substring(0, ipos);
        }
        // formats to 'Language'
        strItem = String.Format(strLang);

        // create a list item object to add to the list
        GUIListItem pItem = new GUIListItem(strItem);

        if (strActive != null) pItem.Label = strItem + " " + strActiveLabel;

        // add it ...
        GUIMessage msg2 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_ADD, GetID, 0,
                                         (int)Controls.OSD_SUBTITLE_LIST, 0, 0, pItem);
        OnMessage(msg2);
      }

      // set the current active subtitle as the selected item in the list control
      if (g_Player.SupportsCC)
      {
        if (currentSubtitleStream == -1)
        {
          GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                             (int)Controls.OSD_SUBTITLE_LIST, 0, 0, null);
          OnMessage(msgSet);
        }
        else
        {
          GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                             (int)Controls.OSD_SUBTITLE_LIST, currentSubtitleStream + 1, 0, null);
          OnMessage(msgSet);
        }
      }
      else
      {
        GUIMessage msgSet = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_SELECT, GetID, 0,
                                           (int)Controls.OSD_SUBTITLE_LIST, g_Player.CurrentSubtitleStream, 0, null);
        OnMessage(msgSet);
      }
    }

    public override void ResetAllControls()
    {
      //reset all
      bool bOffScreen = false;
      int iCalibrationY = GUIGraphicsContext.OSDOffset;
      int iTop = GUIGraphicsContext.OverScanTop;
      int iMin = 0;

      foreach (CPosition pos in _listPositions)
      {
        pos.control.SetPosition((int)pos.XPos, (int)pos.YPos + iCalibrationY);
      }
      foreach (CPosition pos in _listPositions)
      {
        GUIControl pControl = pos.control;

        int dwPosY = pControl.YPosition;
        if (pControl.IsVisible)
        {
          if (dwPosY < iTop)
          {
            int iSize = iTop - dwPosY;
            if (iSize > iMin)
            {
              iMin = iSize;
            }
            bOffScreen = true;
          }
        }
      }
      if (bOffScreen)
      {
        foreach (CPosition pos in _listPositions)
        {
          GUIControl pControl = pos.control;
          int dwPosX = pControl.XPosition;
          int dwPosY = pControl.YPosition;
          if (dwPosY < (int)100)
          {
            dwPosY += Math.Abs(iMin);
            pControl.SetPosition(dwPosX, dwPosY);
          }
        }
      }
      base.ResetAllControls();
    }

    private void Reset()
    {
      GUIPropertyManager.SetProperty("#VideoOSD.AudioVideoDelayPossible", " ");

      // Set all sub menu controls to hidden
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_AUDIO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VIDEO);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_BOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_SUBTITLES);
      HideControl(GetID, (int)Controls.OSD_SUBMENU_BG_VOL);

      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_VOLUMESLIDER_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS);
      HideControl(GetID, (int)Controls.OSD_VIDEOPOS_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOSTREAM_LIST);
      HideControl(GetID, (int)Controls.OSD_AVDELAY);
      HideControl(GetID, (int)Controls.OSD_AVDELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER);
      HideControl(GetID, (int)Controls.OSD_AUDIOVOLUMESLIDER_LABEL);

      HideControl(GetID, (int)Controls.OSD_NONINTERLEAVED);
      HideControl(GetID, (int)Controls.OSD_NOCACHE);
      HideControl(GetID, (int)Controls.OSD_ADJFRAMERATE);

      HideControl(GetID, (int)Controls.OSD_SATURATION);
      HideControl(GetID, (int)Controls.OSD_SATURATIONLABEL);

      HideControl(GetID, (int)Controls.OSD_SHARPNESS);
      HideControl(GetID, (int)Controls.OSD_SHARPNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_BRIGHTNESS);
      HideControl(GetID, (int)Controls.OSD_BRIGHTNESSLABEL);

      HideControl(GetID, (int)Controls.OSD_GAMMA);
      HideControl(GetID, (int)Controls.OSD_GAMMALABEL);

      HideControl(GetID, (int)Controls.OSD_CONTRAST);
      HideControl(GetID, (int)Controls.OSD_CONTRASTLABEL);

      HideControl(GetID, (int)Controls.OSD_CREATEBOOKMARK);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST);
      HideControl(GetID, (int)Controls.OSD_BOOKMARKS_LIST_LABEL);
      HideControl(GetID, (int)Controls.OSD_CLEARBOOKMARKS);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_DELAY_LABEL);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_LIST);
      HideControl(GetID, (int)Controls.OSD_SUBTITLE_FORCED_ONOFF);

      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEBLOCK_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_RESIZE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_DEINTERLACE_ONOFF);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_VERTICAL_LABEL);
      HideControl(GetID, (int)Controls.OSD_VIDEO_POSTPROC_CROP_HORIZONTAL_LABEL);

      ToggleButton((int)Controls.OSD_MUTE, false);
      ToggleButton((int)Controls.OSD_SUBTITLES, false);
      ToggleButton((int)Controls.OSD_BOOKMARKS, false);
      ToggleButton((int)Controls.OSD_VIDEO, false);
      ToggleButton((int)Controls.OSD_AUDIO, false);

      ToggleButton((int)Controls.OSD_REWIND, false); // pop all the relevant
      ToggleButton((int)Controls.OSD_FFWD, false); // buttons back to
      ToggleButton((int)Controls.OSD_PLAY, false); // their up state

      ToggleButton((int)Controls.OSD_SKIPBWD, false); // pop all the relevant
      ToggleButton((int)Controls.OSD_STOP, false); // buttons back to
      ToggleButton((int)Controls.OSD_SKIPFWD, false); // their up state
      ToggleButton((int)Controls.OSD_MUTE, false); // their up state

      //ShowControl(GetID, (int)Controls.OSD_VIDEOPROGRESS);
    }

    public override bool NeedRefresh()
    {
      if (m_bNeedRefresh)
      {
        m_bNeedRefresh = false;
        return true;
      }
      return false;
    }
  }
}
